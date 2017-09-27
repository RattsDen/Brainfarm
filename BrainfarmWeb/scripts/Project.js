// Handlebars template stored in /scripts/Comment.txt
var commentTemplate;
var replyTemplate;
var allComments;
var synthModeEnbled = false;
var synthList;

//*************** DOCUMENT.READY STATEMENTS ******************

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
    var replyBox = $(this).closest(".replyBox");
    var replyText = replyBox.find(".replyText")[0].value;
    var isSpecification = replyBox.find("input[name='chkIsSpecification']")[0].checked;
    var isSynthesis = replyBox.find("input[name='chkIsSynthesis']")[0].checked;
    var isContribution = replyBox.find("input[name='chkIsContribution']")[0].checked;

    if(validateComment(replyText)){
        createCommentWithService(commentid, replyText, isSynthesis, isSpecification, isContribution);
    }
    else {
        replyBox.find(".msg-error").html("Validation failed");
    }
});

$(document).on("click", ".synthDelete", function () {
    $(this).parent().remove();
})

$(document).on("click", "input[name='chkIsSynthesis']", function () {
    var replyBox = $(this).closest(".replyBox");
    //replyBox.find(".synth-list").append("<li>comment</li>");
    replyBox.find(".synthOptions").toggle();
    synthList = replyBox.find(".synthList");
    toggleSynthMode();
})

$(document).on("click", ".commentHead", function () {
    if (synthModeEnbled) {
        addSynth($(this).closest(".comment").data("commentid"));
    }
})

//****************** HELPER FUNCTIONS ********************

function getCommentsFromService(successCallback) {
    var args = {
        projectID: projectID,
        parentCommentID: null
    };
    // serviceAjax function is contained in /scripts/ServiceAjax.js
    return serviceAjax("GetComments", args, successCallback, handleServiceException);
}

function createCommentWithService(commentid, replyText, isSynthesis, isSpecification, isContribution) {
    var args = {
        sessionToken: sessionToken,
        projectID: projectID,
        parentCommentID: commentid,
        bodyText: replyText,
        isSynthesis: isSynthesis,
        isContribution: isSpecification,
        isSpecification: isContribution,
        syntheses: null,
        fileUploads: null
    }

    if (isSynthesis) {
        args.syntheses = getSyntheses();
    }
    serviceAjax("CreateComment", args, reloadAndDisplayAllComments, handleServiceException);
}

function reloadAndDisplayAllComments() {
    getCommentsFromService(processComments);
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
        "url": "/scripts/Reply.html"
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
    $(target).html(commentHTML);
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

function validateComment(replyText) {
    if(replyText.length == 0){
        return false;
    }
    return true;
}

function toggleSynthMode() {
    if (synthModeEnbled === false) {
        synthModeEnbled = true;
    } else {
        synthModeEnbled = false;
    }
}

function addSynth(commentId) {
    var duplicate = false;
    synthList.find("li").each(function () {
        if ($(this).data("commentid") == commentId) {
            duplicate = true;
            return;
        }
    });
    if(!duplicate)
        synthList.append("<li data-commentid='" + commentId + "'><a href='javascript:;' class='synthDelete'>[X]</a>" + commentId + "<input name='synthSubject' type='text' placeholder='Enter a Short Description'/></li>");
}

function getSyntheses() {
    var syntheses = [];
    synthList.find("li").each(function () {
        var commentId = $(this).data("commentid");
        var subject = $(this).find("input[name='synthSubject']")[0].value;
        var tmp = {
            LinkedCommentID: commentId,
            Subject: subject
        };
        syntheses.push(tmp);
    });
    return syntheses;
}