# Network Monitor V2

Simple desktop app to automate checking small network for malfunctions with ICMP ping\
and accessing different services for network nodes.\
All data is saved to sqlite database. Most functions can be accessed through HTTP API.\
Includes chrome browser plugin template (in typesctipt) for adding API functions to custom pages.

## Main features:

Tree structured network nodes database.\
Automated monitoring of online network nodes.\
Automated checks are saved in sessions to permanent history\
with messages of which node not responded and how many its children was skipped.\
Tree view for reference and list view for filtering nodes by tags.\
Access to telnet and ssh if enabled on the node.\
Customizable web services that can be assigned to nodes with different parameters for each.\
&nbsp;&nbsp;&nbsp;&nbsp;For example call to node web interface can be made into template that will use\
&nbsp;&nbsp;&nbsp;&nbsp;node ip and specified (or default for service) port.\
&nbsp;&nbsp;&nbsp;&nbsp;Can be used also to add specific pages for network statistics of the node, some settings linked to node etc.\
&nbsp;&nbsp;&nbsp;&nbsp;All of this available through editor inside the app.

## Building from source
Requirements:\
[.Net Core SDK](https://dotnet.microsoft.com/download)\
[NodeJS](https://nodejs.org/en/)


Process:\
Inside NativeClient/NativeClient.WebUI\
`npm install`\
Inside NativeClient/Electron\
`npm install`\
`npm run prepare-packager`\
`npm run pack:en`\
or\
`npm run pack:ru-ru`\
depending on prefered language (only 2 available at the moment).\
Application for your platform and architecture will be built to
`NetMonV2-{platform}-{arch}` directory inside NativeClient/Electron.\
(Application and build pipeline was tested only on windows platform. May require manual tweaking.)