import { Directive, HostBinding } from '@angular/core';

@Directive({
    // tslint:disable-next-line:directive-selector
    selector: '.nmSizeSetter'
})
export class SizeSetterDirective {
    @HostBinding('style.width')
    width: string;

    @HostBinding('style.height')
    height: string;

    setWidth(width: string) {
        this.width = width + 'px';
    }

    setHeight(height: string) {
        this.height = height + 'px';
    }
}
