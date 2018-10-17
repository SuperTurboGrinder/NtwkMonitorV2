import { SettingsProfileFormComponent } from "./crud/settingsProfileForm.component";
import { SettingsProfileSelectionComponent } from "./crud/settingsProfileSelection.component";
import { TagSelectionComponent } from "./crud/tagSelection.component";
import { TaggedNodeListComponent } from "./taggedNodeList.component";
import { NodesTreeViewComponent } from "./nodesTreeView.component";
import { TagFormComponent } from "./crud/tagForm.component";
import { CustomWebServiceSelectionComponent } from "./crud/customWebServiceSelection.component";


export class UIRoutingConfig {
    public static readonly routes = [
        {
            path: "profiles",
            component: SettingsProfileSelectionComponent,
        },
        {
            path: "profilesSelect",
            component: SettingsProfileSelectionComponent,
        },
        {
            path: "profile/edit/:id",
            component: SettingsProfileFormComponent
        },
        {
            path: "profile/new",
            component: SettingsProfileFormComponent
        },
        {
            path: "tags",
            component: TagSelectionComponent
        },
        {
            path: "tag/new",
            component: TagFormComponent
        },
        {
            path: "tag/edit/:id",
            component: TagFormComponent
        },
        {
            path: "customWebServices",
            component: CustomWebServiceSelectionComponent
        },
        {
            path: "tagFilteredView",
            component: TaggedNodeListComponent
        },
        {
            path: "treeView",
            component: NodesTreeViewComponent
        },
        {
            path: "",
            redirectTo: "/profilesSelect",
            pathMatch: "full"
        }
    ]
}