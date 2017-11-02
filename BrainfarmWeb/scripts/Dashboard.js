$(document).ready(function () {

    // Project clicked - go to project
    $(document).on("click", ".div-project", function (event) {
        var projectID = $(event.target).closest(".div-project").data("project-id");
        window.location.href = "/Project.aspx?ID=" + projectID;
    });

});
