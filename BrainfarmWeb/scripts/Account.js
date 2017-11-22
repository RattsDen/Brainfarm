var commentTemplate;

$(document).ready(function () {
    $.when(getUserComments(), getUserBookmarkedComments(), getCommentTemplate())
        .done(function (commentsResp, bookmarksResp, templateResp) {
            if (commentsResp[1] == "success" && bookmarksResp[1] == "success" && templateResp[1] == "success") {
                console.log(commentsResp);
                prepareCommentTemplate(templateResp[0]);
                processComments(commentsResp[0], "#div-comments-list");
                processComments(bookmarksResp[0], "#div-bookmarks-list");
            }
        });

    $(document).on("click", "#btn-user-projects", function () {
        showTab("#btn-user-projects", "#div-projects");
    });

    $(document).on("click", "#btn-user-comments", function () {
        showTab("#btn-user-comments", "#div-comments");
    });

    $(document).on("click", "#btn-user-bookmarks", function () {
        showTab("#btn-user-bookmarks", "#div-bookmarks");
    });

    $(document).on("click", "#btn-edit-account", function () {
        showTab("#btn-edit-account", "#div-edit-account");
    });

    // Project clicked - go to project
    $(document).on("click", ".div-project", function (event) {
        var projectID = $(event.target).closest(".div-project").data("project-id");
        window.location.href = "/Project.aspx?ID=" + projectID;
    });

});

function showTab(buttonId, tabId) {
    $("#div-tabs .btn").removeClass("btn-dark");
    $(buttonId).addClass("btn-dark");

    $(".tab").hide();
    $(tabId).show();
}

function getUserComments() {
    var args = {
        userID: userID
    };
    return serviceAjax("GetUserComments", args, null, null);
}

function getUserBookmarkedComments() {
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
