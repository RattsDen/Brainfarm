$(document).ready(function () {

});

function showProjectPage(projectId) {
    // request ASPX webpage that displays a project for the given projectId
    window.location.href = "Project.aspx?ID=" + projectId;
}
