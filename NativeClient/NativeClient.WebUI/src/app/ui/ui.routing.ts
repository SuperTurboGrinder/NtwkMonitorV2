import { Route } from '@angular/router';
import { SettingsProfileFormComponent } from './crud/settingsProfileForm.component';
import { SettingsProfileSelectionComponent } from './crud/settingsProfileSelection.component';
import { TagSelectionComponent } from './crud/tagSelection.component';
import { TaggedNodeListComponent } from './taggedNodeList.component';
import { NodesTreeViewComponent } from './nodesTreeView.component';
import { TagFormComponent } from './crud/tagForm.component';
import { CustomWebServiceSelectionComponent } from './crud/customWebServiceSelection.component';
import { CustomWebServiceFormComponent } from './crud/customWebServiceForm.component';
import { NtwkNodeSelectionComponent } from './crud/ntwkNodeSelection.component';
import { NtwkNodeFormComponent } from './crud/ntwkNodeForm.component';
import { TagsBindingSideSelectorComponent } from './crud/tagsBindingSideSelector.component';
import { CustomWebServiceBindingSideSelectorComponent } from './crud/customWebServiceBindingSideSelector.component';
import { HubUIComponent } from './hubUI.component';
import { EditorComponent } from './crud/editor.component';
import { FormHostComponent } from './crud/formHost.component';
import { TagFilterComponent } from './tagFilter.component';
import { HistoryViewComponent } from './historyView.component';
import { TaggedNodesListUpdateListenerComponent } from './misc/taggedNodesListUpdateListener.component';
import { NodesTreeViewUpdateListenerComponent } from './misc/nodesTreeViewUpdatesListener.component';


export class UIRoutingConfig {
    private static readonly crudFormsRoutes: Route[] = [
        {
            path: 'profile/new',
            component: SettingsProfileFormComponent
        },
        {
            path: 'profile/edit/:id',
            component: SettingsProfileFormComponent
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
            path: 'customWebService/new',
            component: CustomWebServiceFormComponent
        },
        {
            path: 'customWebService/edit/:id',
            component: CustomWebServiceFormComponent
        },
        {
            path: 'node/new',
            component: NtwkNodeFormComponent
        },
        {
            path: 'node/:parentId/new',
            component: NtwkNodeFormComponent
        },
        {
            path: 'node/edit/:id',
            component: NtwkNodeFormComponent
        },
        {
            path: 'tagsBinding',
            component: TagsBindingSideSelectorComponent
        },
        {
            path: 'customWebServicesBinding',
            component: CustomWebServiceBindingSideSelectorComponent
        }
    ];

    private static readonly crudEditorRoutes: Route[] = [
        {
            path: 'profiles',
            component: SettingsProfileSelectionComponent,
        },
        {
            path: 'profilesSelect',
            component: SettingsProfileSelectionComponent,
        },
        {
            path: 'tags',
            component: TagSelectionComponent
        },
        {
            path: 'customWebServices',
            component: CustomWebServiceSelectionComponent
        },
        {
            path: 'nodes',
            component: NtwkNodeSelectionComponent
        }
    ];

    private static readonly hubRoutes: Route[] = [
        {
            path: '',
            redirectTo: 'initialProfilesSelect',
            pathMatch: 'full'
        },
        {
            path: 'editor',
            component: EditorComponent,
            children: UIRoutingConfig.crudEditorRoutes
        },
        {
            path: 'form',
            component: FormHostComponent,
            children: UIRoutingConfig.crudFormsRoutes
        },

        // data display coponents
        {
            path: 'initialProfilesSelect',
            component: SettingsProfileSelectionComponent,
        },
        {
            path: 'tagFilteredView',
            component: TaggedNodesListUpdateListenerComponent
        },
        {
            path: 'filterConfiguration',
            component: TagFilterComponent
        },
        {
            path: 'treeView',
            component: NodesTreeViewUpdateListenerComponent
        },
        {
            path: 'historyView',
            component: HistoryViewComponent
        }
    ];

    public static routes: Route[] = [
        {
            path: '',
            redirectTo: 'ui',
            pathMatch: 'full'
        },
        {
            path: 'ui',
            component: HubUIComponent,
            children: UIRoutingConfig.hubRoutes
            // redirectTo: '/profilesSelect',
            // pathMatch: 'full'
        }
    ];
}
