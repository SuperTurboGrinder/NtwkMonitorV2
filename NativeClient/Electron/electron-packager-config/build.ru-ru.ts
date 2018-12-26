import { BuildPipeline } from './buildPipeline';
import { packageElectronApp } from './electron-packager.common';

packageElectronApp(
    'ru-ru',
    (pipeline: BuildPipeline) => pipeline.buildAllUnbuiltSubprojects()
);
