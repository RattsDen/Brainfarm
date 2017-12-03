$(document).ready(function () {

    $.when(getUserBookmarkedComments(), getCommentTemplate())
        .done(function (bookmarksResp, templateResp) {
            if (bookmarksResp && bookmarksResp[1] == "success" && templateResp[1] == "success") {
                prepareCommentTemplate(templateResp[0]);
                mostRecentBookmarks = bookmarksResp[0].slice(0, 5); // Show only top 5
                processComments(mostRecentBookmarks, "#div-bookmarks-list");
            }
        });

    // Project clicked - go to project
    $(document).on("click", ".div-project", function (event) {
        var projectID = $(event.target).closest(".div-project").data("project-id");
        window.location.href = "/Project.aspx?ID=" + projectID;
    });

});

function getUserBookmarkedComments() {
    // Don't make request if user is not logged in
    if (sessionToken == null || sessionToken == "")
        return null;

    var args = {
        sessionToken: sessionToken
    };
    return serviceAjax("GetUserBookmarkedComments", args, null, null);
}

function getCommentTemplate() {
    return $.ajax({
        "type": "GET",
        "url": "/scripts/CommentAccount.html"
    });
}

function prepareCommentTemplate(templateText) {
    Handlebars.registerHelper("parseMSDate", parseMSDate);
    commentTemplate = Handlebars.compile(templateText);
}

function processComments(comments, targetId) {
    var target = $(targetId);
    var commentHTML = layoutComments(comments);
    $(target).html(commentHTML);
    //console.log(comments);
}

function layoutComments(comments) {
    return commentTemplate(comments);
}

function parseMSDate(datestring) {
    var millis = parseInt(datestring.substring(6));
    var date = new Date(millis);

    // Format date as yyyy-MM-dd h:mm p
    // JavaScript dates are a wonderful thing
    var yyyy = date.getFullYear();
    var MM = date.getMonth() + 1;
    MM = MM < 10 ? "0" + MM : MM;
    var dd = date.getDate();
    dd = dd < 10 ? "0" + dd : dd;
    var h = date.getHours();
    var p = h >= 12 ? "PM" : "AM";
    h = h == 0 ? 12 : h > 12 ? h - 12 : h;
    var mm = date.getMinutes();
    mm = mm < 10 ? "0" + mm : mm;

    return yyyy + "-" + MM + "-" + dd + " " + h + ":" + mm + " " + p;
}
