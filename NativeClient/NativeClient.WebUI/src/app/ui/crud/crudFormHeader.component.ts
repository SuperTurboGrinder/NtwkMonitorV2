import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-crud-form-header',
    templateUrl: './crudFormHeader.component.html'
})
export class CrudFormHeaderComponent {
    @Input() isEditMode = false;
}
