import { NgModule, InjectionToken } from '@angular/core';
import { HttpClientModule, HttpClient } from '@angular/common/http';

import { ModelModule } from '../model/model.module';
import { MessagingService } from './messaging.service';
import { PingCacheService } from './pingCache.service';
import { HTTPDatasource } from './http.datasource';
import { PingService } from './ping.service';
import { ExecServicesService } from './execServices.service';
import { WebServicesService } from './webServices.service';
import { NodesService } from './nodes.service';
import { TagsService } from './tags.service';
import { SettingsProfilesService } from './settingsProfiles.service';
import { BaseURL } from './baseUrl.token';
import { NodeInfoPopupDataService } from './nodeInfoPopupData.service';
import { TreeCollapsingService } from './treeCollapsing.service';
import { CustomWebServicesService } from './customWebServices.service';
import { SoundNotificatonService } from './soundNotificationService';
import { PingMonitorService } from './pingMonitor.service';

@NgModule({
  declarations: [
  ],
  imports: [
    ModelModule,
    HttpClientModule
  ],
  providers: [
    { provide: BaseURL, useValue: 'http://localhost:5001/api/' },
    HttpClient,
    TreeCollapsingService,
    NodeInfoPopupDataService,
    MessagingService,
    PingCacheService,
    HTTPDatasource,
    PingService,
    ExecServicesService,
    WebServicesService,
    NodesService,
    TagsService,
    SettingsProfilesService,
    CustomWebServicesService,
    SoundNotificatonService,
    PingMonitorService
  ],
})
export class ServicesModule { }
