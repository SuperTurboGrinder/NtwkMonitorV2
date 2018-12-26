import * as packager from 'electron-packager';
import { BuildPipeline } from './buildPipeline';

export function packageElectronApp(
    locale: string,
    buildFunction: (pipeline: BuildPipeline) => Promise<boolean>
) {
    let buildPipeline: BuildPipeline = null;
    packager({
        dir: 'app',
        executableName: 'NetworkMonitorV2',
        name: 'NetMonitorV2',
        // icon: 'icon.png',
        asar: true,
        afterExtract: [async (buildPath: string, electronVersion: string,
        platform: string, arch: string, callback: ()=>void) => {
            buildPipeline = new BuildPipeline(locale, platform, arch);
            const built = await buildFunction(buildPipeline);
            if (built) {
                await buildPipeline.copyWebAPITo(buildPath);
                await buildPipeline.copyAPIDatabaseTo(buildPath);
                callback();
            } else {
                console.log('Failed to build app component projects.');
            }
        }],
        // afterCopy: [(buildPath, electronVersion, platform, arch, callback) => {}],
        afterPrune: [(buildPath: string, electronVersion: string,
        platform: string, arch: string, callback: ()=>void) => {
            buildPipeline.copyWebUITo(buildPath).then(() => callback());
        }]
    })
    .then((appPath: string | string[]) => {
        console.log(`Electron app was packaged to ${appPath}`);
    })
    .catch(err => {
        console.log('Unexpected exception occured while building electron app.');
    });
}
