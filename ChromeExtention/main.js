function requestToAPI(path) {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", "http://localhost:5000/"+path, true);
    xhr.send();
    return xhr;
}

function openSSH(nodeID) {
    var request = requestToAPI("nodes/"+nodeID+"/ssh");
    request.onload = () => console.log("OPENED SSH");
}

function openTelnet(nodeID) {
    var request = requestToAPI("nodes/"+nodeID+"/telnet");
    request.onload = () => console.log("OPENED TELNET");
}

function openWeb(nodeID) {
    var request = requestToAPI("nodes/"+nodeID+"/web");
    request.onload = () => console.log("OPENED WEB INTERFACE");
}

function config_buttons(data) {
    use_button_list(buttons => {
        var usable_buttons = buttons.map(bl => {
            var bdata = data.find(d => d.name == bl.nodeName);
            if(bdata == undefined) {
                bl.telnet.remove();
                bl.ssh.remove();
                bl.web.remove();
                return null;
            }
            else {
                if(bl.telnet != null && !bdata.hasTelnet) {
                    bl.telnet.remove();
                    bl.telnet = null;
                }
                if(bl.web != null && !bdata.hasWeb) {
                    bl.web.remove();
                    bl.web = null;
                }
                if(bl.ssh != null && !bdata.hasSSH) {
                    bl.ssh.remove();
                    bl.ssh = null
                }
                if(bl.telnet == null &&
                    bl.web == null &&
                    bl.ssh == null
                ) {
                    return null;
                }
                return {
                    telnet:bl.telnet,
                    web:bl.web,
                    ssh:bl.ssh,
                    id:bdata.id
                };
            }
        }).filter(b => b != null);
        usable_buttons.forEach(ub => {
            if(ub.telnet != null) {
                ub.telnet.addEventListener('click', () => openTelnet(ub.id));
            }
            if(ub.web != null) {
                ub.web.addEventListener('click', () => openWeb(ub.id));
            }
            if(ub.ssh != null) {
                ub.ssh.addEventListener('click', () => openSSH(ub.id));
            }
        });
    });
}

function main() {
    var requestForNodes = requestToAPI("nodes/data/all");
    
    requestForNodes.onload = function () {
        var result = JSON.parse(this.responseText);
        var ntwkNodes = result.map(node => {
            if(node.isBlackBox) {
                return null;
            }
            return {
                id: node.id,
                name: node.name,
                hasSSH: node.isOpenSSH,
                hasTelnet: node.isOpenTelnet,
                hasWeb: node.isOpenWeb,
            }
        }).filter(n => n != null);
        config_buttons(ntwkNodes);
    }
}

main();