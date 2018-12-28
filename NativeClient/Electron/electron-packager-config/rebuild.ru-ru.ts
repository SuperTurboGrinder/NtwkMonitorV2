import { ElectronPackagerScript, PipelineType } from './electron-packager.common';

const packagerScript =
    new ElectronPackagerScript('ru-ru', PipelineType.RebuildAndPack);
packagerScript.packageElectronApp();
