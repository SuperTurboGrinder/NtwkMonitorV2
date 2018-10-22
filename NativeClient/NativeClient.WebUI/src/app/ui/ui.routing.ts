import { SettingsProfileFormComponent } from './crud/settingsProfileForm.component';
import { SettingsProfileSelectionComponent } from './crud/settingsProfileSelection.component';
import { TagSelectionComponent } from './crud/tagSelection.component';
import { TaggedNodeListComponent } from './taggedNodeList.component';
import { NodesTreeViewComponent } from './nodesTreeView.component';
import { TagFormComponent } from './crud/tagForm.component';
import { CustomWebServiceSelectionComponent } from './crud/customWebServiceSelection.component';
import { CustomWebServiceFormComponent } from './crud/customWebServiceForm.component';
import { NtwkNodeSelectionComponent } from './crud/ntwkNodeSelection.component';


export class UIRoutingConfig {
    public static readonly routes = [
        {
            path: 'profiles',
            component: SettingsProfileSelectionComponent,
        },
        {
            path: 'profilesSelect',
            component: SettingsProfileSelectionComponent,
        },
        {
            path: 'profile/edit/:id',
            component: SettingsProfileFormComponent
        },
        {
            path: 'profile/new',
            component: SettingsProfileFormComponent
        },
        {
            path: 'tags',
            component: TagSelectionComponent
        },
        {
            path: 'tag/new',
            component: TagFormComponent
        },
        {
            path: 'tag/edit/:id',
            component: TagFormComponent
        },
        {
            path: 'customWebServices',
            component: CustomWebServiceSelectionComponent
        },
        {
            path: 'customWebService/new',
            component: CustomWebServiceFormComponent
        },
        {
            path: 'customWebService/edit/:id',
            component: CustomWebServiceFormComponent
        },
        {
            path: 'tagFilteredView',
            component: TaggedNodeListComponent
        },
        {
            path: 'treeView',
            component: NodesTreeViewComponent
        },
        {
            path: 'nodes',
            component: NtwkNodeSelectionComponent
        },
        {
            path: '',
            redirectTo: '/profilesSelect',
            pathMatch: 'full'
        }
    ];
}
