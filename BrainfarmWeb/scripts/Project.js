// Handlebars template stored in /scripts/Comment.txt
var commentTemplate;
var replyTemplate;
var allComments;

$(document).ready(function () {
    // Make AJAX calls to get comments from service and to get the comment template
    // Wait for both to finish before processing the responses
    $.when(getCommentsFromService(), getCommentTemplate(), getReplyTemplate())
        .done(function (commentResp, commentTemplateResp, replyTemplateResp) {
            if (commentResp[1] == "success" && commentTemplateResp[1] == "success" && replyTemplateResp[1] == "success") {
                prepareTemplate(commentTemplateResp[0]);
                processComments(commentResp[0]);
                replyTemplate = replyTemplateResp[0];
            }
        });
});

$(document).on("click", ".btnReply", function () {
    var comment = $(this).closest(".commentContent");
    if (!comment.hasClass("hasReplyForm")) {
        comment.append(replyTemplate);
        comment.addClass("hasReplyForm");
    }
});

$(document).on("click", ".btn-cancelReply", function () {
    var comment = $(this).closest(".commentContent");
    comment.find(".replyBox").remove();
    comment.removeClass("hasReplyForm");
});

$(document).on("click", ".btn-submitReply", function () {
    var comment = $(this).closest(".comment");
    var commentid = comment.data("commentid");
    createCommentWithService(commentid);
});

function getCommentsFromService() {
    var args = {
        projectID: projectID,
        parentCommentID: null
    };
    // serviceAjax function is contained in /scripts/ServiceAjax.js
    return serviceAjax("GetComments", args, null, handleServiceException);
}

function createCommentWithService(commentid) {
    var args = {

    }
    //serviceAjax("CreateComment");
}

function getCommentTemplate() {
    return $.ajax({
        "type": "GET",
        "url": "/scripts/Comment.txt"
    });
}

function getReplyTemplate() {
    return $.ajax({
        "type": "GET",
        "url": "/scripts/Reply.txt"
    });
}

function handleServiceException(fault) {
    // TODO: Better exception handling
    console.log(fault);
}

function prepareTemplate(templateText) {
    Handlebars.registerHelper("layoutChildren", layoutComments);
    Handlebars.registerHelper("parseMSDate", parseMSDate);
    commentTemplate = Handlebars.compile(templateText);
}

// Get a comment by its id. This is not making a service call, 
//it is just finding it in the already loaded list of comments.
function getCommentByID(comments, id) {
    var result = null;
    for (var i = 0; i < comments.length; i++) {
        if (comments[i].CommentID == id) {
            result = comments[i];
            break;
        } else {
            result = getCommentByID(comments[i].Children, id);
            if (result != null) {
                break;
            }
        }
    }
    return result;
}

function processComments(comments) {
    allComments = comments;

    var target = $("#div-project-comments");

    var commentHTML = layoutComments(comments);
    $(target).append(commentHTML);
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