/*
Scripts for providing interaction with the project search page
*/

$(document).ready(function () {

    $(document).on("click", ".div-project", function (event) {
        var projectID = $(event.target).closest(".div-project").data("project-id");
        window.location.href = "/Project.aspx?ID=" + projectID;
    });

});
