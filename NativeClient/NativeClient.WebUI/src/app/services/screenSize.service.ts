import { Injectable, HostListener, OnInit } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';

import { ScreenSize } from '../model/viewModel/screenSize.model';

@Injectable()
export class ScreenSizeService implements OnInit {
    private screenSize = new BehaviorSubject<ScreenSize>({
        width: 800,
        height: 600
    });

    public subscribeToScreenSize(
        func: (screenSizes: {width: number; height: number}) => void
    ): Subscription {
        return this.screenSize.subscribe(func);
    }

    ngOnInit() {
        this.updateScreenSize();
    }

    @HostListener('window:resize', ['$event'])
    onResize() {
        this.updateScreenSize();
    }

    private updateScreenSize() {
        this.screenSize.next(
            {width: window.innerWidth, height: window.innerHeight}
         );
    }
}
