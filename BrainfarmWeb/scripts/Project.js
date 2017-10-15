/*
Scripts for providing interaction with the Project page incuding filtering
*/

// TODO: Refactor this file into several smaller ones
//       -display comments
//       -filtering
//       -reply/edit/delete
//       -bookmarking

// Handlebars template stored in /scripts/Comment.txt
var commentTemplate;
var replyTemplate;
var editTemplate;
var allComments;
var bookmarkedCommentIDs = [];
var synthModeEnbled = false;
var synthList;
var currentUser;

//*************** DOCUMENT.READY STATEMENTS ******************

var pendingFileUploads = [];

$(document).ready(function () {
    // Make AJAX calls to get comments from service and to get the comment template
    // Wait for both to finish before processing the responses
    $.when(getCommentsFromService(), getBookmarksFromService(), getCurrentUserFromService(), getCommentTemplate(), getReplyTemplate(), getEditTemplate())
        .done(function (commentResp, bookmarksResp, currentUserResp, commentTemplateResp, replyTemplateResp, editTemplateResp) {
            if (commentResp[1] == "success"
                    && commentTemplateResp[1] == "success"
                    && replyTemplateResp[1] == "success") {

                // bookmarksResp will be null if the call is not made
                //(null will be given to the .when() if the call is not to be made)
                if (bookmarksResp && bookmarksResp[1] == "success") {
                    bookmarkedCommentIDs = bookmarksResp[0];
                }
                //Same goes for currentUserResp
                if (currentUserResp){
                    if (currentUserResp[1] == "success") {
                        currentUser = currentUserResp[0];
                    }
                }else {
                    currentUser = currentUserResp;
                }

                prepareTemplate(commentTemplateResp[0]);
                processComments(commentResp[0]);
                replyTemplate = replyTemplateResp[0];
                editTemplate = editTemplateResp[0];
            }

            $(".btnReply").click(function () {
                $this = $(this);
                parentCommentId = $this.closest(".comment").data("commentid");
                $("#BodyContentPlaceHolder_parentCommentId").attr("value", parentCommentId);
                $("#lblParentCommentId").text(parentCommentId);
            });

        });

    $(document).on("click", ".attachments a", attachmentClicked);
});

$(document).on("click", ".btnReply", function () {
    closeAllCommentForms();
    var comment = $(this).closest(".commentContent");
    if (!comment.hasClass("hasReplyForm")) {
        comment.append(replyTemplate);
        comment.addClass("hasReplyForm");

        pendingFileUploads = []; // Reinitialize the pending file upload list
    }
});

$(document).on("click", ".btnEdit", function () {
    closeAllCommentForms();
    var comment = $(this).closest(".commentContent");
    if (!comment.hasClass("hasEditForm")) {
        comment.append(editTemplate);
        comment.addClass("hasEditForm");
    }
    comment.find(".replyBox").find(".replyText")[0].value = comment.find(".commentBody>p").text();
});

$(document).on("click", ".btnRemove", function () {
    var comment = $(this).closest(".comment");
    var commentid = comment.data("commentid");
    if (confirm("Are you sure?")) {
        removeCommentWithService(commentid);
    }
});

$(document).on("click", ".btn-cancelReply", function () {
    toggleSynthMode(false);
    var comment = $(this).closest(".commentContent");
    comment.find(".replyBox").remove();
    comment.removeClass("hasReplyForm");
});

$(document).on("click", ".btn-cancelEdit", function () {
    toggleSynthMode(false);
    var comment = $(this).closest(".commentContent");
    comment.find(".replyBox").remove();
    comment.removeClass("hasEditForm");
});

$(document).on("click", ".btn-submitReply", function () {
    var comment = $(this).closest(".comment");
    var commentid = comment.data("commentid");
    var replyBox = $(this).closest(".replyBox");
    var replyText = replyBox.find(".replyText")[0].value;
    var isSpecification = replyBox.find("input[name='chkIsSpecification']")[0].checked;
    var isSynthesis = replyBox.find("input[name='chkIsSynthesis']")[0].checked;
    var isContribution = replyBox.find("input[name='chkIsContribution']")[0].checked;
    var errorDisplay = replyBox.find(".msg-error");
    var syntheses = null;
    errorDisplay.html("");
    $(".missingField").remove();

    if (isSynthesis) {
        syntheses = getSyntheses();
        if (syntheses.length < 2) {
            errorDisplay.append("Cannot synthesize fewer than 2 comments<br/>");
            return;
        }
        if (syntheses.hasErrors) {
            errorDisplay.append("Please include a subject for each sythesized comment</br>");
            return;
        }
    }

    if(validateComment(errorDisplay, replyText)){
        createCommentWithService(commentid, replyText, isSynthesis, isSpecification, isContribution, syntheses);
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

$(document).on("click", ".btn-submitEdit", function () {
    var comment = $(this).closest(".comment");
    var commentid = comment.data("commentid");
    var replyBox = $(this).closest(".replyBox");
    var replyText = replyBox.find(".replyText")[0].value;
    var isSpecification = replyBox.find("input[name='chkIsSpecification']")[0].checked;
    var isSynthesis = replyBox.find("input[name='chkIsSynthesis']")[0].checked;
    var isContribution = replyBox.find("input[name='chkIsContribution']")[0].checked;
    var errorDisplay = replyBox.find(".msg-error");
    var syntheses = null;
    errorDisplay.html("");
    $(".missingField").remove();

    if (isSynthesis) {
        syntheses = getSyntheses();
        if (syntheses.length < 2) {
            errorDisplay.append("Cannot synthesize fewer than 2 comments<br/>");
            return;
        }
        if (syntheses.hasErrors) {
            errorDisplay.append("Please include a subject for each sythesized comment</br>");
            return;
        }
    }

    if (validateComment(errorDisplay, replyText)) {
        editCommentWithService(commentid, replyText, isSynthesis, isSpecification, isContribution, syntheses);
    }
});

$(document).on("click", ".btnLoginMessage", function () {
    $("#txtUsername").focus();
})

// BEGIN Contribution comment listeners

// "Is Contribution Comment" checkbox click
$(document).on("click", ".chk-is-contribution", function () {
    var checkbox = $(event.target);
    var divFileUpload = $(".div-file-upload");
    divFileUpload.toggleClass("reply-box-hidden", checkbox.checked);
});

// "Add a file" button clicked
$(document).on("click", ".btn-add-file", function () {
    var control = "<input type='file' class='input-file-upload btn btn-green' />";
    var divFileUploadInputs = $(".div-file-upload-inputs");
    divFileUploadInputs.append(control);
    divFileUploadInputs.append("<br />");

    return false; //Prevent ASP.NET postback
});

// File selected
$(document).on("change", ".file-upload", function () {
    var fileInputControl = $(event.target);
    var fileInputLabel = $(".file-upload-label");

    pendingFileUploads = [];
    var requests = [];
    var labelText = "";

    for (var i = 0; i < fileInputControl[0].files.length; i++) {
        var file = fileInputControl[0].files[i];

        // Build label text
        if (i > 0)
            labelText += ", ";
        labelText += file.name;

        // start the upload and add it to list of requests
        requests.push(uploadFile(file))
    }

    fileInputLabel.text("Uploading...")

    $.when.apply(undefined, requests).then(function () {
        fileInputLabel.text(labelText);
    });
});

function uploadFile(file) {
    return $.ajax({
        url: serviceEndpoint + "UploadFile",
        type: "POST",
        data: file,
        processData: false,
        success: function (response) {
            console.log("upload done");
            pendingFileUploads.push({
                ContributionFileID: response.ContributionFileID,
                Filename: file.name
            });
        },
        error: function () {
            console.log("upload error");
        }
    });
}

// END Contribution comment listeners

//****************** HELPER FUNCTIONS ********************

function getCommentsFromService(successCallback) {
    var args = {
        projectID: projectID,
        parentCommentID: null
    };
    // serviceAjax function is contained in /scripts/ServiceAjax.js
    return serviceAjax("GetComments", args, successCallback, handleServiceException);
}

function getBookmarksFromService() {
    if (sessionToken != null && sessionToken != "") {
        var args = {
            sessionToken: sessionToken,
            projectID: projectID
        };
        return serviceAjax("GetBookmarksForProject", args, null, handleServiceException);
    } else {
        console.log("NO TOKEN")
        return null;
    }
}

function getCurrentUserFromService() {
    if (sessionToken != null && sessionToken != "") {
        var args = {
            sessionToken: sessionToken
        };
        return serviceAjax("GetCurrentUser", args, null, handleServiceException);
    } else {
        console.log("NO TOKEN")
        return null;
    }
}

function createCommentWithService(commentid, replyText, isSynthesis, isSpecification, isContribution, syntheses) {
    var args = {
        sessionToken: sessionToken,
        projectID: projectID,
        parentCommentID: commentid,
        bodyText: replyText,
        isSynthesis: isSynthesis,
        isContribution: isContribution,
        isSpecification: isSpecification,
        syntheses: syntheses,
        attachments: isContribution ? pendingFileUploads : null
    }

    serviceAjax("CreateComment", args, reloadAndDisplayAllComments, handleServiceException);
}

function editCommentWithService(commentid, replyText, isSynthesis, isSpecification, isContribution, syntheses) {
    var args = {
        sessionToken: sessionToken,
        commentID: commentid,
        bodyText: replyText,
        isSynthesis: isSynthesis,
        isContribution: isSpecification,
        isSpecification: isContribution
    }

    serviceAjax("EditComment", args, reloadAndDisplayAllComments, handleServiceException);
}

function removeCommentWithService(commentid) {
    var args = {
        sessionToken: sessionToken,
        commentID: commentid
    }

    serviceAjax("RemoveComment", args, reloadAndDisplayAllComments, handleServiceException);
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

function getEditTemplate() {
    return $.ajax({
        "type": "GET",
        "url": "/scripts/Edit.html"
    });
}

function handleServiceException(fault) {
    // TODO: Better exception handling
    console.log(fault);
    var responseText = $.parseXML(fault.responseText);
    var faultExceptionMessage = $(responseText).find("Text").text();
    alert(faultExceptionMessage);
}

function prepareTemplate(templateText) {
    Handlebars.registerHelper("layoutChildren", layoutComments);
    Handlebars.registerHelper("parseMSDate", parseMSDate);
    Handlebars.registerHelper("isBookmarked", isBookmarked);
    Handlebars.registerHelper("isCurrentUser", isCurrentUser);
    Handlebars.registerHelper("isUserLoggedIn", isUserLoggedIn);
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

function isUserLoggedIn(options) {
    if (options) {
        if (currentUser) {
            return options.fn(this); //true
        }
        return options.inverse(this); // false
    } else {
        if (currentUser) {
            return true;
        }
        return false;
    }
}

function isCurrentUser(userID, options) {
    if (isUserLoggedIn()) {
        if (userID == currentUser.UserID) {
            return options.fn(this); // true
        }
    }
        
    return options.inverse(this); // false
}

function isBookmarked(commentID, options) {
    if (bookmarkedCommentIDs && bookmarkedCommentIDs.includes(commentID)) {
        return options.fn(this); // true
    } else {
        return options.inverse(this); // false
    }
}

function attachmentClicked(event) {
    // Get file id and filename from clicked tag
    var contributionFileId = $(event.target).data("contribution-file-id");
    var filename = $(event.target).data("filename");

    // Direct to download controller
    location.href = "/DownloadFile.ashx?ID=" + contributionFileId + "&Filename=" + filename;

}

//TODO: Add better validation?
function validateComment(errorDisplay, replyText) {
    if (replyText.length == 0) {
        errorDisplay.append("Reply text cannot be empty<br/>");
        return false;
    }
    return true;
}

function toggleSynthMode(override) {
    if (override === true || override === false) {
        synthModeEnbled = override;
        return;
    }
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
    if (!duplicate) {
        synthList.append(
            "<li data-commentid='" + commentId + "'>" +
                "<a href='javascript:;' class='synthDelete'>[X]</a>" +
                commentId +
                "<input name='synthSubject' type='text' placeholder='Enter a Short Description'/>" +
            "</li>"
        );
    }
}

function getSyntheses() {
    var syntheses = [];
    synthList.find("li").each(function (k) {
        var commentId = $(this).data("commentid");
        var subject = $(this).find("input[name='synthSubject']")[0].value;
        var tmp = {
            LinkedCommentID: commentId,
            Subject: subject
        };
        syntheses.push(tmp);
        if (subject == "") {
            $(this).append("<span class='missingField'>*</span>");
            syntheses.hasErrors = true;
        }
    });
    console.log(syntheses);
    return syntheses;
}

function closeAllCommentForms(){
    $(".hasReplyForm").each(function () {
        $(this).find(".btn-cancelReply").trigger("click");
    });
    $(".hasEditForm").each(function () {
        $(this).find(".btn-cancelEdit").trigger("click");
    });
}



$(document).on("click", "#btn-filter-apply", function () {
    // All comments is technically an array, even though 
    //it should only have one element being the root of the tree
    for (var i = 0; i < allComments.length; i++) {
        filterCommentTree(allComments[i]);
    }
});

$(document).on("click", "#btn-filter-remove", function () {
    // All comments is technically an array, even though 
    //it should only have one element being the root of the tree
    for (var i = 0; i < allComments.length; i++) {
        removeFiltersFromCommentTree(allComments[i]);
    }
});


// Applies filter styling to a tree of comments
// input: root of the tree
// returns: 
function filterCommentTree(node) {
    // Recurse through tree, leaf-first

    var anyChildIsVisible = false;
    if (node.Children != null) {
        for (var i = 0; i < node.Children.length; i++) {
            var thisChildIsVisible = filterCommentTree(node.Children[i]);
            anyChildIsVisible = anyChildIsVisible || thisChildIsVisible;
        }
    }

    var commentElement = $("#" + node.CommentID);

    // Process node
    if (commentMatchesFilter(node)) {
        // Set classes for visible
        commentElement.removeClass("filter-hidden");
        commentElement.removeClass("filter-faded");
        return true;
    } else if (anyChildIsVisible) {
        // Set classes for faded
        commentElement.removeClass("filter-hidden");
        commentElement.addClass("filter-faded");
        return true;
    } else {
        // Set classes for hidden
        commentElement.addClass("filter-hidden");
        commentElement.removeClass("filter-faded");
        return false;
    }
}

function commentMatchesFilter(comment) {
    // Get filter settings from checkboxes
    var showNormal = $("#chk-filter-normal").is(":checked");
    var showSynth = $("#chk-filter-synth").is(":checked");
    var showSpec = $("#chk-filter-spec").is(":checked");
    var showContrib = $("#chk-filter-contrib").is(":checked");
    var showBookmarked = $("#chk-filter-bookmarked").is(":checked");

    return comment.ParentCommentID == null // Allways show first comment
        || (showNormal && !comment.IsSynthesis && !comment.IsSpecification && !comment.IsContribution)
		|| (showSynth && comment.IsSynthesis)
		|| (showSpec && comment.IsSpecification)
		|| (showContrib && comment.IsContribution)
        || (showBookmarked && bookmarkedCommentIDs.includes(comment.CommentID));
}

function removeFiltersFromCommentTree(node) {
    // Recurse through tree
    if (node.Children != null) {
        for (var i = 0; i < node.Children.length; i++) {
            removeFiltersFromCommentTree(node.Children[i]);
        }
    }
    // Set comments to visible
    var commentElement = $("#" + node.CommentID);
    commentElement.removeClass("filter-hidden");
    commentElement.removeClass("filter-faded");
}

$(document).on("click", ".btnBookmark", function () {
    var commentView = $(this).closest(".comment");
    var commentID = commentView.data("commentid");
    var args = {
        sessionToken: sessionToken,
        commentID: commentID
    };

    if (bookmarkedCommentIDs && bookmarkedCommentIDs.includes(commentID)) {
        // Is currently bookmarked
        serviceAjax("UnbookmarkComment", args, function () {
            // Update stored model
            // remove from array
            bookmarkedCommentIDs.splice(bookmarkedCommentIDs.indexOf(commentID), 1);
            // Update view
            commentView.find(".bookmark:first").text("");
            commentView.find(".btnBookmark:first").text("Bookmark");
        }, handleServiceException, "text");
    } else {
        // Not bookmarked
        serviceAjax("BookmarkComment", args, function () {
            // Update stored model
            bookmarkedCommentIDs.push(commentID);
            // Update view
            commentView.find(".bookmark:first").text("Bookmarked");
            commentView.find(".btnBookmark:first").text("Unbookmark");
        }, handleServiceException, "text");
    }
});
