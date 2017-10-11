var serviceEndpoint = "http://localhost:59006/BrainfarmService.svc/ajax/";

function serviceAjax(route, args, success, error, dataType) {

    // Stringify arguments object
    if (typeof args == "object") {
        args = JSON.stringify(args);
    }

    if (dataType == undefined) {
        dataType = "json";
    }
    
    return $.ajax({
        "type": "POST",
        "url": serviceEndpoint + route,
        "data": args,
        "contentType": "application/json",
        "dataType": dataType,
        "success": function (response) {
            if(success != undefined)
                success(response);
        },
        "error": error,
        "processData": false
    });

}