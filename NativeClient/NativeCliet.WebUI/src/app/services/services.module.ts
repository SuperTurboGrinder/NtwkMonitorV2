import { NgModule, InjectionToken } from '@angular/core';
import { HttpClientModule, HttpClient } from '@angular/common/http';

import { ModelModule } from "../model/model.module";
import { MessagingService } from "./messaging.service";
import { UpdateAlarmService } from "./updateAlarm.service";
import { HTTPDatasource } from "./http.datasource";
import { NodesRepository } from "./nodes.repository";
import { BaseURL } from "./baseUrl.token";

@NgModule({
  declarations: [
  ],
  imports: [
    ModelModule,
    HttpClientModule
  ],
  providers: [
    { provide: BaseURL, useValue: "http://localhost:5001/api/" },
    HttpClient,
    MessagingService,
    UpdateAlarmService,
    HTTPDatasource,
    NodesRepository
  ],
})
export class ServicesModule { }