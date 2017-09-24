// Handlebars template stored in /scripts/Comment.txt
var commentTemplate;
var allComments;
var commentReplyOverlay;

$(document).ready(function () {
    commentReplyOverlay = $("#commentReplyOverlay");
    // Make AJAX calls to get comments from service and to get the comment template
    // Wait for both to finish before processing the responses
    $.when(getCommentsFromService(), getCommentTemplate())
        .done(function (commentResp, templateResp) {
            if (commentResp[1] == "success" && templateResp[1] == "success") {
                prepareTemplate(templateResp[0]);
                processComments(commentResp[0]);
            }

            $(".btnReply").click(function () {
                $this = $(this);
                parentCommentId = $this.closest(".comment").data("commentid");
                $("#BodyContentPlaceHolder_parentCommentId").attr("value", parentCommentId);
                $("#lblParentCommentId").text(parentCommentId);
                commentReplyOverlay.removeClass("hidden");
            })
        });

    $(document).on("click", ".attachments a", attachmentClicked);
});

function getCommentsFromService() {
    var args = {
        projectID: projectID,
        parentCommentID: null
    };
    // serviceAjax function is contained in /scripts/ServiceAjax.js
    return serviceAjax("GetComments", args, null, handleServiceException);
}

function getCommentTemplate() {
    return $.ajax({
        "type": "GET",
        "url": "/scripts/Comment.txt"
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

function attachmentClicked(event) {
    // Get file id and filename from clicked tag
    var contributionFileId = $(event.target).data("contribution-file-id");
    var filename = $(event.target).data("filename");

    // Direct to download controller
    location.href = "/DownloadFile.ashx?ID=" + contributionFileId + "&Filename=" + filename;

}
