import * as packager from 'electron-packager';

packager({
    dir: 'app',
    executableName: 'NetworkMonitorV2',
    name: 'NetMonitorV2',
    //icon: 'icon.png',
    asar: true,
    afterExtract: [(buildPath: string, electronVersion: string,
    platform: string, arch: string, callback: ()=>void) => {
        console.log("afterExtract:");
        console.log(buildPath);
        console.log(electronVersion);
        console.log(platform);
        console.log(arch);
        callback();
    }],
    afterCopy: [(buildPath: string, electronVersion: string,
    platform: string, arch: string, callback: ()=>void) => {
        console.log("afterCopy:");
        console.log(buildPath);
        console.log(electronVersion);
        console.log(platform);
        console.log(arch);
        callback();
    }],
    afterPrune: [(buildPath: string, electronVersion: string,
    platform: string, arch: string, callback: ()=>void) => {
        console.log("afterPrune:");
        console.log(buildPath);
        console.log(electronVersion);
        console.log(platform);
        console.log(arch);
        callback();
    }]
})
.then((appPath: string | string[]) => { /* … */ });