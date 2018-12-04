import { Component, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { pipe, Subscription } from 'rxjs';
import { filter, map, merge, mergeMap } from 'rxjs/operators';
import { Title } from '@angular/platform-browser';

@Component({
    selector: 'app-hub-ui-main',
    templateUrl: './hubUI.component.html'
})
export class HubUIComponent implements OnDestroy {
    private _hostingForms = true;
    private readonly routerSubscription: Subscription;
    private baseTitleString = 'NetworkMonitorV2';

    public get hostingForms() {
        return this._hostingForms;
    }

    constructor(
        router: Router,
        private titleService: Title,
        private activatedRoute: ActivatedRoute
    ) {
        this. routerSubscription = router.events.pipe(
            filter(event => event instanceof NavigationEnd),
            map(_ => this.activatedRoute.firstChild),
            filter(child => child !== null),
            mergeMap(route => route.url)
        ).subscribe(event => {
            if (event[0].path !== 'profilesSelect') {
                this._hostingForms = event[0].path === 'form';
            }
        });
        this.setTitleSuffix('Tree View');
    }

    public ngOnDestroy() {
        this.routerSubscription.unsubscribe();
    }

    public setTitleSuffix(suffix: string) {
        this.titleService.setTitle(`${this.baseTitleString}  [${suffix}]`);
    }
}
