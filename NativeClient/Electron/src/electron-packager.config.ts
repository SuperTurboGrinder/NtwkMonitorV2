import * as packager from 'electron-packager';

packager({
    dir: 'app',
    executableName: 'NetworkMonitorV2',
    name: 'NetMonitorV2',
    //icon: 'icon.png',
    asar: true,
    afterExtract: [(buildPath, electronVersion, platform, arch, callback) => {
        console.log("afterExtract:");
        console.log(buildPath);
        console.log(electronVersion);
        console.log(platform);
        console.log(arch);
        callback();
    }],
    afterCopy: [(buildPath, electronVersion, platform, arch, callback) => {
        console.log("afterCopy:");
        console.log(buildPath);
        console.log(electronVersion);
        console.log(platform);
        console.log(arch);
        callback();
    }],
    afterPrune: [(buildPath, electronVersion, platform, arch, callback) => {
        console.log("afterPrune:");
        console.log(buildPath);
        console.log(electronVersion);
        console.log(platform);
        console.log(arch);
        callback();
    }]
})
.then(appPaths => { /* … */ });