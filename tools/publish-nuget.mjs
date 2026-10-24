#!/usr/bin/env node
/**
 * Packs the MajProtocol NuGet packages.
 *
 * Three packages are shipped:
 *
 *   MajProtocol.Standard                  (packed first; the others depend on it)
 *   MajProtocol.Client.WebSocketSharp
 *   MajProtocol.Server.WebSocketSharp
 *
 * MajProtocol.Standard.Generator is a build-time source generator. It is NOT a
 * package of its own: it only feeds the MajProtocol.Standard build, so the
 * generated packet types and routing interfaces are already compiled into
 * MajProtocol.Standard.dll. Consumers reference MajProtocol.Standard and get
 * those types from the assembly; the generator itself is never shipped.
 *
 * This script only packs. Pushing to nuget.org stays a manual, deliberate step;
 * the exact commands are printed at the end.
 *
 * The version comes from Directory.Build.props. Bump it there to release.
 *
 * Usage:
 *   node tools/publish-nuget.mjs [options]
 *
 * Options:
 *   -c, --configuration <name>  Build configuration (default: Release)
 *   -o, --output <dir>          Output directory (default: artifacts/packages)
 *       --no-clean              Keep existing *.nupkg files in the output directory
 *       --dry-run               Print the commands without running them
 *   -h, --help                  Show this help
 */

import { spawnSync } from 'node:child_process';
import { existsSync, mkdirSync, readFileSync, readdirSync, rmSync, statSync } from 'node:fs';
import { dirname, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { inflateRawSync } from 'node:zlib';

const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');

const GENERATOR_ID = 'MajProtocol.Standard.Generator';

/** Pack order matters: consumers declare a dependency on MajProtocol.Standard. */
const PACKAGES = [
    {
        id: 'MajProtocol.Standard',
        project: 'MajProtocol.Standard/MajProtocol.Standard.csproj',
        dependsOn: [],
    },
    {
        id: 'MajProtocol.Client.WebSocketSharp',
        project: 'MajProtocol.Client.WebSocketSharp/MajProtocol.Client.WebSocketSharp.csproj',
        dependsOn: ['MajProtocol.Standard'],
    },
    {
        id: 'MajProtocol.Server.WebSocketSharp',
        project: 'MajProtocol.Server.WebSocketSharp/MajProtocol.Server.WebSocketSharp.csproj',
        dependsOn: ['MajProtocol.Standard'],
    },
];

const NUGET_SOURCE = 'https://api.nuget.org/v3/index.json';

// ---------------------------------------------------------------------------
// Minimal ZIP reader: a .nupkg is a zip, and we want to verify its contents
// without shelling out to unzip or pulling in a dependency.
// ---------------------------------------------------------------------------

function openZip(buf) {
    let eocd = -1;
    for (let i = buf.length - 22; i >= 0 && i >= buf.length - 22 - 0xffff; i--) {
        if (buf.readUInt32LE(i) === 0x06054b50) {
            eocd = i;
            break;
        }
    }

    if (eocd < 0)
        throw new Error('not a zip archive (end of central directory not found)');

    const entryCount = buf.readUInt16LE(eocd + 10);
    let offset = buf.readUInt32LE(eocd + 16);
    const entries = new Map();

    for (let i = 0; i < entryCount; i++) {
        if (buf.readUInt32LE(offset) !== 0x02014b50)
            throw new Error('corrupt zip central directory');

        const method = buf.readUInt16LE(offset + 10);
        const compressedSize = buf.readUInt32LE(offset + 20);
        const nameLength = buf.readUInt16LE(offset + 28);
        const extraLength = buf.readUInt16LE(offset + 30);
        const commentLength = buf.readUInt16LE(offset + 32);
        const localOffset = buf.readUInt32LE(offset + 42);
        const name = buf.toString('utf8', offset + 46, offset + 46 + nameLength);

        entries.set(name, { method, compressedSize, localOffset });
        offset += 46 + nameLength + extraLength + commentLength;
    }

    return {
        names: [...entries.keys()],
        readText(name) {
            const entry = entries.get(name);
            if (!entry)
                return null;

            const nameLength = buf.readUInt16LE(entry.localOffset + 26);
            const extraLength = buf.readUInt16LE(entry.localOffset + 28);
            const start = entry.localOffset + 30 + nameLength + extraLength;
            const data = buf.subarray(start, start + entry.compressedSize);

            if (entry.method === 0)
                return data.toString('utf8');
            if (entry.method === 8)
                return inflateRawSync(data).toString('utf8');

            throw new Error(`unsupported zip compression method ${entry.method} for ${name}`);
        },
    };
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function fail(message) {
    console.error(`\n  error  ${message}\n`);
    process.exit(1);
}

function parseArgs(argv) {
    const options = {
        configuration: 'Release',
        output: 'artifacts/packages',
        clean: true,
        dryRun: false,
        help: false,
    };

    for (let i = 0; i < argv.length; i++) {
        const arg = argv[i];

        switch (arg) {
            case '-c':
            case '--configuration':
                options.configuration = argv[++i] ?? fail(`${arg} requires a value`);
                break;
            case '-o':
            case '--output':
                options.output = argv[++i] ?? fail(`${arg} requires a value`);
                break;
            case '--no-clean':
                options.clean = false;
                break;
            case '--dry-run':
                options.dryRun = true;
                break;
            case '-h':
            case '--help':
                options.help = true;
                break;
            default:
                fail(`unknown argument "${arg}" (try --help)`);
        }
    }

    return options;
}

function printHelp() {
    const text = readFileSync(fileURLToPath(import.meta.url), 'utf8');
    const usage = text.slice(text.indexOf(' * Packs'), text.indexOf(' */'));
    console.log(usage.split('\n').map(line => line.replace(/^ \* ?/, '')).join('\n'));
}

/** Reads <Version> from Directory.Build.props, the single source of truth. */
function readVersion() {
    const propsPath = join(repoRoot, 'Directory.Build.props');
    if (!existsSync(propsPath))
        fail('Directory.Build.props not found; it holds the package version');

    const match = readFileSync(propsPath, 'utf8').match(/<Version>([^<]+)<\/Version>/);
    if (!match)
        fail('no <Version> element in Directory.Build.props');

    return match[1].trim();
}

function run(command, args, options = {}) {
    if (options.dryRun) {
        console.log(`  $ ${[command, ...args].join(' ')}`);
        return;
    }

    const result = spawnSync(command, args, {
        cwd: repoRoot,
        stdio: 'inherit',
        // No shell: `dotnet` is a real executable, and shell:true would only
        // concatenate the arguments unescaped (DEP0190).
        shell: false,
    });

    if (result.error)
        fail(`failed to run ${command}: ${result.error.message}`);
    if (result.status !== 0)
        fail(`${command} exited with code ${result.status}`);
}

function formatBytes(bytes) {
    if (bytes < 1024)
        return `${bytes} B`;
    if (bytes < 1024 * 1024)
        return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

// ---------------------------------------------------------------------------
// Package verification: the checks that catch a broken release.
// ---------------------------------------------------------------------------

function verifyPackage(nupkgPath, pkg, version) {
    const problems = [];
    const zip = openZip(readFileSync(nupkgPath));
    const nuspecName = `${pkg.id}.nuspec`;
    const nuspec = zip.readText(nuspecName);

    if (!nuspec)
        return [`${nuspecName} is missing from the package`];

    // 1. Identity and version must match what we meant to ship.
    const id = nuspec.match(/<id>([^<]+)<\/id>/)?.[1];
    const nuspecVersion = nuspec.match(/<version>([^<]+)<\/version>/)?.[1];

    if (id !== pkg.id)
        problems.push(`nuspec id is "${id}", expected "${pkg.id}"`);
    if (nuspecVersion !== version)
        problems.push(`nuspec version is "${nuspecVersion}", expected "${version}"`);

    // 2. Metadata that makes the package presentable on nuget.org.
    for (const [label, pattern] of [
        ['description', /<description>[^<]+<\/description>/],
        ['authors', /<authors>[^<]+<\/authors>/],
        ['license', /<license /],
        ['projectUrl', /<projectUrl>[^<]+<\/projectUrl>/],
        ['repository url', /<repository [^>]*url="[^"]+"/],
        ['readme', /<readme>[^<]+<\/readme>/],
    ]) {
        if (!pattern.test(nuspec))
            problems.push(`nuspec is missing ${label}`);
    }

    // 3. The assembly itself.
    const libEntry = `lib/netstandard2.1/${pkg.id}.dll`;
    if (!zip.names.includes(libEntry))
        problems.push(`${libEntry} is missing from the package`);

    // 4. The readme the nuspec advertises must really be packed.
    if (!zip.names.includes('README.md'))
        problems.push('README.md is missing from the package');

    // 5. The generator is build-time only. Its output is already compiled into
    //    MajProtocol.Standard.dll, so shipping the analyzer as well would make
    //    consumers generate the same types twice (CS0436).
    const strayAnalyzers = zip.names.filter(name => name.startsWith('analyzers/'));

    for (const entry of strayAnalyzers)
        problems.push(`${entry} must not be packed: the generator is build-time only`);

    // 6. Declared dependencies must match the packages in this release, and the
    //    generator must never be a package dependency.
    const declared = [...nuspec.matchAll(/<dependency\s+[^>]*id="([^"]+)"[^>]*version="([^"]+)"/g)]
        .map(([, dependencyId, dependencyVersion]) => ({ dependencyId, dependencyVersion }));

    for (const dependency of pkg.dependsOn) {
        const match = declared.find(entry => entry.dependencyId === dependency);

        if (!match)
            problems.push(`nuspec does not depend on ${dependency}`);
        else if (!match.dependencyVersion.includes(version))
            problems.push(
                `nuspec depends on ${dependency} ${match.dependencyVersion}, expected ${version}`);
    }

    if (declared.some(entry => entry.dependencyId === GENERATOR_ID))
        problems.push(`nuspec must not declare a dependency on ${GENERATOR_ID}`);

    return problems;
}

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------

function main() {
    const options = parseArgs(process.argv.slice(2));

    if (options.help) {
        printHelp();
        return;
    }

    const version = readVersion();
    const outputDir = resolve(repoRoot, options.output);

    console.log(`\nMajProtocol NuGet pack`);
    console.log(`  version        ${version}`);
    console.log(`  configuration  ${options.configuration}`);
    console.log(`  output         ${relative(repoRoot, outputDir) || '.'}`);
    console.log(`  packages       ${PACKAGES.map(pkg => pkg.id).join(', ')}`);
    console.log(`  generator      build-time only, compiled into MajProtocol.Standard (not published)`);

    if (!options.dryRun) {
        if (options.clean && existsSync(outputDir)) {
            for (const entry of readdirSync(outputDir)) {
                if (entry.endsWith('.nupkg') || entry.endsWith('.snupkg'))
                    rmSync(join(outputDir, entry), { force: true });
            }
        }
        mkdirSync(outputDir, { recursive: true });
    }

    // Pack in dependency order so a consumer package can never be built against
    // a MajProtocol.Standard that does not exist yet.
    for (const pkg of PACKAGES) {
        const projectPath = join(repoRoot, pkg.project);
        if (!existsSync(projectPath))
            fail(`project not found: ${pkg.project}`);

        console.log(`\n  pack   ${pkg.id}`);
        run('dotnet',
            ['pack', pkg.project,
                '-c', options.configuration,
                '-o', outputDir,
                '--nologo'],
            { dryRun: options.dryRun });
    }

    if (options.dryRun) {
        console.log('\nDry run: nothing was packed.\n');
        return;
    }

    console.log(`\nVerifying packages`);
    const produced = [];
    let failed = false;

    for (const pkg of PACKAGES) {
        const nupkgPath = join(outputDir, `${pkg.id}.${version}.nupkg`);

        if (!existsSync(nupkgPath)) {
            console.error(`  fail   ${pkg.id}.${version}.nupkg was not produced`);
            failed = true;
            continue;
        }

        const problems = verifyPackage(nupkgPath, pkg, version);
        const size = formatBytes(statSync(nupkgPath).size);

        if (problems.length === 0) {
            console.log(`  ok     ${pkg.id}.${version}.nupkg (${size})`);
        } else {
            console.error(`  fail   ${pkg.id}.${version}.nupkg (${size})`);
            for (const problem of problems)
                console.error(`           - ${problem}`);
            failed = true;
        }

        produced.push({ pkg, nupkgPath });
    }

    // Nothing else should have been packed: the generator is not a package.
    const stray = readdirSync(outputDir)
        .filter(entry => entry.endsWith('.nupkg'))
        .filter(entry => !PACKAGES.some(pkg => entry.startsWith(`${pkg.id}.`)));

    for (const entry of stray) {
        console.error(`  fail   unexpected package ${entry} (the generator must not be published)`);
        failed = true;
    }

    if (failed)
        fail('packaging checks failed; the packages are not ready to publish');

    console.log(`\nDone. ${produced.length} packages are ready in ${relative(repoRoot, outputDir)}`);
    console.log(`Pushing is manual. Publish in this order, MajProtocol.Standard first:`);
    console.log();

    for (const { nupkgPath } of produced)
        console.log(`  dotnet nuget push "${relative(repoRoot, nupkgPath)}" --source ${NUGET_SOURCE} --api-key $NUGET_API_KEY`);

    console.log();
    console.log(`  ($NUGET_API_KEY is %NUGET_API_KEY% in cmd.exe)`);
    console.log();
}

main();
