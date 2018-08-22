import { NgModule } from '@angular/core';

import { ModelModule } from "../model/model.module";
import { MessagingService } from "./messaging.service";

@NgModule({
  declarations: [
  ],
  imports: [
  ],
  providers: [
    MessagingService
  ],
})
export class ServicesModule { }