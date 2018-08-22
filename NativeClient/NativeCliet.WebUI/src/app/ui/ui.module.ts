import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { ModelModule } from "../model/model.module";
import { BackendErrorSelectorComponent } from './messeging/backendErrorSelector.component';
import { MessageDisplayComponent } from './messeging/messageDisplay.component';

@NgModule({
  declarations: [
    BackendErrorSelectorComponent,
    MessageDisplayComponent
  ],
  imports: [
    BrowserAnimationsModule, CommonModule, ModelModule
  ],
  exports: [
    MessageDisplayComponent
  ],
  providers: [],
})
export class UIModule { }