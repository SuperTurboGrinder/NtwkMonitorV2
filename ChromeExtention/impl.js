//this file is your implementation
//use_button_list is your interface to this plugin
//just create button/anchors or whatever
//element you like for plugin to add onclick
//listeners to them and give them to callback function
//like in this template code
//code in main.js will do everything else
//any of the buttons returned can be null to skip

function createButton(container, text) {
    var button = document.createElement("button");
    button.type = "button";
    button.innerHTML = text;
    container.appendChild(button);
    return button;
}

//template implementation
//rewrite for yor own case
function use_button_list(callback) {
    var containers = Array.prototype.slice.call(document.getElementsByTagName("div"));
    var buttons = [];
    containers.forEach(container => {
        var nodeName = container.innerText.trim();
        while(container.firstChild) {
            container.firstChild.remove();
        }
        var telnet = createButton(container, "T");
        var web = createButton(container, "W");
        var ssh = createButton(container, "SSH");
        buttons.push({
            telnet,
            web,
            ssh,
            nodeName
        });
    });
    callback(buttons);
}