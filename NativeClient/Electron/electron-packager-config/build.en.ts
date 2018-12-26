import { BuildPipeline } from './buildPipeline';
import { packageElectronApp } from './electron-packager.common';

packageElectronApp(
    'en',
    (pipeline: BuildPipeline) => pipeline.buildAllUnbuiltSubprojects()
);
