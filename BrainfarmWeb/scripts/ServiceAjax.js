function serviceAjax(route, args, success, error) {

    // Stringify arguments object
    if (typeof args == "object") {
        args = JSON.stringify(args);
    }
    
    return $.ajax({
        "type": "POST",
        "url": "http://localhost:59006/BrainfarmService.svc/ajax/" + route,
        "data": args,
        "contentType": "application/json",
        "dataType": "json",
        "success": success,
        "error": error,
        "processData": false
    });

}