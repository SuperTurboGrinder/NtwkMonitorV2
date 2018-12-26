import { Component } from '@angular/core';
import { SettingsProfilesService } from '../../services/settingsProfiles.service';
import { SettingsProfile } from '../../model/httpModel/settingsProfile.model';
import { ActivatedRoute, Router } from '@angular/router';
import { MessagingService } from 'src/app/services/messaging.service';
import { BaseCrudSelectorComponent } from '../helpers/baseCrudSelectorComponent.helper';

@Component({
    selector: 'app-settings-profile-selection',
    templateUrl: './settingsProfileSelection.component.html'
})
export class SettingsProfileSelectionComponent
    extends BaseCrudSelectorComponent<SettingsProfile, SettingsProfilesService> {
    private _isEditorView: boolean;
    private readonly pageName: string;

    public get isEditorView() {
        return this._isEditorView;
    }

    constructor(
        messager: MessagingService,
        private router: Router,
        private settingsService: SettingsProfilesService,
        route: ActivatedRoute
    ) {
        super(messager, settingsService);
        this.pageName = route.snapshot.url[0].path;
        this.setIsEditorView();
    }

    private setIsEditorView() {
        this._isEditorView = this.pageName !== 'profilesSelect'
        && this.pageName !== 'initialProfilesSelect';
    }

    public setAndContinue(profileID: number) {
        if (!this.isCurrentProfile(profileID)) {
            this.settingsService.setCurrentProfile(profileID);
            this.router.navigateByUrl('ui/treeView');
        }
    }

    private moveToMakingNewProfile() {
        this.router.navigateByUrl('/ui/form/profile/new');
    }

    public isCurrentProfile(id: number): boolean {
        return this.settingsService.isCurrentProfileID(id);
    }

    protected updateDataList() {
        this.dataService.getProfiles().subscribe(
            tagsListResult => {
                this.setIsEditorView();
                if (this.isEditorView === false) {
                    this.setNewData(tagsListResult);
                    if (this.dataList.length === 0) {
                        this.moveToMakingNewProfile();
                    } else if (this.dataList.length === 1) {
                        this.setAndContinue(this.dataList[0].id);
                    }
                } else {
                    tagsListResult.data = tagsListResult.success === true
                        ? tagsListResult.data.filter(
                            p => this.dataService.isCurrentProfileID(p.id) === false
                        )
                        : tagsListResult.data;
                    this.setNewData(tagsListResult);
                }
            }
        );
    }

    protected deleteObjectPermanently(
        objID: number,
        callback: (success: boolean) => void
    ) {
        this.dataService.deleteProfile(objID, callback);
    }
}
