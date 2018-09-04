import { Component } from '@angular/core';
import { Observable, interval } from 'rxjs';

import { BackendErrorStatuses } from './model/httpModel/backendErrorStatuses.model'
import { MessagingService } from './services/messaging.service';
import { SettingsProfilesService } from './services/settingsProfiles.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'Network Monitor V2';
  testInterval: Observable<number>;

  constructor(
    private messaging: MessagingService
  ) {
    
  }
}
