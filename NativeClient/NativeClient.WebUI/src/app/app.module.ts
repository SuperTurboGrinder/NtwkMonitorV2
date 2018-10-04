import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { UIModule } from './ui/ui.module';
import { ServicesModule } from './services/services.module';

import { AppComponent } from './app.component';
import { RoutingConfig } from './app.routing'

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule, UIModule, ServicesModule,
    RouterModule,
    RouterModule.forRoot(RoutingConfig.routes)
  ],
  providers: [
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
