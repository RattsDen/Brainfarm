function serviceAjax(method, args, success, error) {

    // Stringify arguments object
    if (typeof args == "object") {
        args = JSON.stringify(args);
    }
    
    $.ajax({
        "type": "POST",
        "url": "http://localhost:59006/BrainfarmService.svc/ajax/" + method,
        "data": args,
        "contentType": "application/json",
        "dataType": "json",
        "success": success,
        "error": error,
        "processData": false
    });

}