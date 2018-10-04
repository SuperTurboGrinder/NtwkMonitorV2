import { SettingsProfileFormComponent } from "./ui/crud/settingsProfileForm.component";
import { SettingsProfileSelectionComponent } from "./ui/settingsProfileSelection.component";


export class RoutingConfig {
    public static readonly routes = [
        {
            path: "profiles",
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
            path: "",
            redirectTo: "/profiles",
            pathMatch: "full"
        }
    ]
}