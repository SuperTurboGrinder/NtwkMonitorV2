import { ElectronPackagerScript, PipelineType } from './electron-packager.common';

const packagerScript =
    new ElectronPackagerScript('en', PipelineType.RebuildAndPack);
packagerScript.packageElectronApp();
