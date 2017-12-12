// Handlebars template stored in /scripts/Comment.txt
var commentTemplate;
var replyTemplate;
var editTemplate;
var allComments;
var bookmarkedCommentIDs = [];
var userRatings = [];
var synthModeEnabled = false;
var synthList;
var currentUser;

//*************** DOCUMENT.READY STATEMENTS ******************

var pendingFileUploads = [];

$(document).ready(function () {
    // Make AJAX calls to get comments from service and to get the comment template
    // Wait for both to finish before processing the responses

    // Make several AJAX calls to get data such as comments and HTML templates
    // Wait for all calls to complete before processing the responses
    $.when(
        getCommentsFromService(),    // Comments
        getBookmarksFromService(),   // Current user's bookmarks (optional)
        getUserRatingsFromService(), // Current user's rated comments (optional)
        getCurrentUserFromService(), // Current user info (optional)
        getCommentTemplate(),        // Comment HTML template
        getReplyTemplate(),          // Reply box HTML template
        getEditTemplate()            // Edit box HTML template
    ).done(function (
        commentResp,
        bookmarksResp,
        userRatingsResp,
        currentUserResp,
        commentTemplateResp,
        replyTemplateResp,
        editTemplateResp)
    {
        if (commentResp[1] == "success"
                && commentTemplateResp[1] == "success"
                && replyTemplateResp[1] == "success") {
        
            // bookmarksResp will be null if the call is not made
            //(null will be given to the .when() if the call is not to be made)
            if (bookmarksResp && bookmarksResp[1] == "success") {
                bookmarkedCommentIDs = bookmarksResp[0];
            }
            //Same goes for userRatingsResp
            if (userRatingsResp && userRatingsResp[1] == "success") {
                userRatings = userRatingsResp[0];
            }
            //Same goes for currentUserResp
            if (currentUserResp){
                if (currentUserResp[1] == "success") {
                    currentUser = currentUserResp[0];
                }
            } else {
                currentUser = currentUserResp;
            }
        
            prepareTemplate(commentTemplateResp[0]);
            processComments(commentResp[0]);
            replyTemplate = replyTemplateResp[0];
            editTemplate = editTemplateResp[0];

            // Scroll to comment once loaded, if so specified in the query parameters
            var queryParams = {};
            location.search.substr(1).split("&").forEach(function (item) {
                queryParams[item.split("=")[0]] = item.split("=")[1]
            });
            if (queryParams["Comment"]) { // If parameter is specified
                $("#" + queryParams["Comment"])[0].scrollIntoView(true); // Scroll to comment
            }
        }
    });


    // Reply button pressed
    $(document).on("click", ".btnReply", function () {
        closeAllCommentForms();
        var comment = $(this).closest(".commentContent");
        if (!comment.hasClass("hasReplyForm")) {
            comment.append(replyTemplate);
            comment.addClass("hasReplyForm");

            pendingFileUploads = []; // Reinitialize the pending file upload list
        }
    });

    // Edit button pressed
    $(document).on("click", ".btnEdit", function () {
        closeAllCommentForms();
        var commentContent = $(this).closest(".commentContent");
        if (!commentContent.hasClass("hasEditForm")) {
            commentContent.append(editTemplate);
            commentContent.addClass("hasEditForm");
        }
        commentContent.find(".replyBox").find(".replyText")[0].value = commentContent.find(".commentBody>p").text();
        var comment = $(this).closest(".comment");
        if (comment.hasClass("synth")) {
            comment.find("input[name='chkIsSynthesis']").trigger("click");
            //find the synth links elements, but only use the first one. otherwise any child synth comments will be included
            comment.find(".commentBody .synth-links").first().find("a").each(function () {
                addSynth($(this).data("commentid"), $(this).data("subject"));
            });
        }
    });

    // Remove button pressed
    $(document).on("click", ".btnRemove", function () {
        var comment = $(this).closest(".comment");
        var commentid = comment.data("commentid");
        if (confirm("Are you sure?")) {
            removeCommentWithService(commentid);
        }
    });

    // Bookmark button pressed
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
                commentView.find(".bookmark:first").removeClass("fa");
                commentView.find(".bookmark:first").removeClass("fa-bookmark");
                commentView.find(".btnBookmark:first").removeClass("pressed");
            }, handleServiceException, "text");
        } else {
            // Not bookmarked
            serviceAjax("BookmarkComment", args, function () {
                // Update stored model
                bookmarkedCommentIDs.push(commentID);
                // Update view
                commentView.find(".bookmark:first").addClass("fa");
                commentView.find(".bookmark:first").addClass("fa-bookmark");
                commentView.find(".btnBookmark:first").addClass("pressed");
            }, handleServiceException, "text");
        }
    });

    // Like button pressed
    $(document).on("click", ".btnLike", function () {
        var commentView = $(this).closest(".comment");
        var commentID = commentView.data("commentid");
        var commentModel = getCommentByID(allComments, commentID);
        var args = {
            sessionToken: sessionToken,
            commentID: commentID
        };

        var ratingPosition = null; // Position of rating in the userRatings array
        if (userRatings) {
            for (var i = 0; i < userRatings.length; i++) {
                if (userRatings[i].CommentID == commentID) {
                    ratingPosition = i;
                    break;
                }
            }
        }

        // ratingPosition will be null if the comment is not liked yet
        if (ratingPosition != null) {
            // Is currently liked
            serviceAjax("RemoveRating", args, function (removedRating) {
                // Update stored model
                userRatings.splice(ratingPosition, 1); // Remove from array
                commentModel.Score -= removedRating.Weight;
                // Update view
                commentView.find(".btnLike:first").removeClass("pressed");
                commentView.find(".score:first").text(commentModel.Score != 0 ? commentModel.Score : "");
            }, handleServiceException);
        } else {
            // Is not currently liked
            serviceAjax("AddRating", args, function (addedRating) {
                // Update stored model
                userRatings.push(addedRating);
                commentModel.Score += addedRating.Weight;
                // Update view
                commentView.find(".btnLike:first").addClass("pressed");
                commentView.find(".score:first").text(commentModel.Score != 0 ? commentModel.Score : "");
            }, handleServiceException);
        }

    });

    // "Log in to reply" button pressed
    $(document).on("click", ".btnLoginMessage", function () {
        $("#txtUsername").focus();
    });


    //Cancel Reply button pressed
    $(document).on("click", ".btn-cancelReply", function () {
        toggleSynthMode(false);
        var comment = $(this).closest(".commentContent");
        comment.find(".replyBox").remove();
        comment.removeClass("hasReplyForm");
    });

    // Cancel Edit button pressed
    $(document).on("click", ".btn-cancelEdit", function () {
        toggleSynthMode(false);
        var comment = $(this).closest(".commentContent");
        comment.find(".replyBox").remove();
        comment.removeClass("hasEditForm");
    });

    // Submit reply button pressed
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

        if (validateComment(errorDisplay, replyText)) {
            createCommentWithService(commentid, replyText, isSynthesis, isSpecification, isContribution, syntheses);
        }
    });

    // Submit edit pressed
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

    // "Is Synthesis" checkbox pressed
    $(document).on("click", "input[name='chkIsSynthesis']", function () {
        var replyBox = $(this).closest(".replyBox");
        //replyBox.find(".synth-list").append("<li>comment</li>");
        replyBox.find(".synthOptions").toggle();
        synthList = replyBox.find(".synthList");
        toggleSynthMode();
    });

    // Comment header pressed
    // (for synthesis) TODO: change to either checkbox or button for choosing comments to synthesise
    $(document).on("click", ".btnSynthesizeComment", function () {
        if (synthModeEnabled) {
            addSynth($(this).closest(".comment").data("commentid"));
            $(this).addClass("pressed");
        }
    });

    // Remove synthesis button pressed
    $(document).on("click", ".synthDelete", function () {
        $(this).parent().remove();
    });

    // Edit Project button pressed
    $(document).on("click", ".btn-edit-project", function () {
        // Hide project info div
        $("#div-project-title").hide();
        // Show edit project div
        $("#div-project-edit").show();
    });

    // Cancel Edit Project button Pressed
    $(document).on("click", "#btnEditProjectCancel", function () {
        // Hide edit project div
        $("#div-project-edit").hide();
        // Show project info div
        $("#div-project-title").show();
    });
});



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
        //console.log("NO TOKEN");
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
        //console.log("NO TOKEN");
        return null;
    }
}

function getUserRatingsFromService() {
    if (sessionToken != null && sessionToken != "") {
        var args = {
            sessionToken: sessionToken,
            projectID: projectID
        };
        return serviceAjax("GetUserRatings", args, null, handleServiceException);
    } else {
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
        isContribution: isContribution,
        isSpecification: isSpecification,
        syntheses: syntheses,
        attachments: isContribution ? pendingFileUploads : null
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
    toggleSynthMode(false);
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

// Handle an exception thrown by the web service during an AJAX call
function handleServiceException(fault) {
    // TODO: Better exception handling
    console.log(fault);
    var responseText = $.parseXML(fault.responseText);
    var faultExceptionMessage = $(responseText).find("Text").text();
    alert(faultExceptionMessage);
}

// Prepare the Handlebars comment template
function prepareTemplate(templateText) {
    Handlebars.registerHelper("layoutChildren", layoutComments);
    Handlebars.registerHelper("parseMSDate", parseMSDate);
    Handlebars.registerHelper("isBookmarked", isBookmarked);
    Handlebars.registerHelper("isCurrentUser", isCurrentUser);
    Handlebars.registerHelper("isUserLoggedIn", isUserLoggedIn);
    Handlebars.registerHelper("isLiked", isLiked);
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

// Place comments on the page
function processComments(comments) {
    allComments = comments;
    var target = $("#div-project-comments");
    var commentHTML = layoutComments(comments);
    $(target).html(commentHTML);
}

// Create HTML for an array of comments using the Handlebars template
// returns: an HTML string
function layoutComments(comments) {
    return commentTemplate(comments);
}

// Handlebars helper for parsing Microsoft JSON date format
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

// Handlebars helper function for checking if the user is logged in
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

// Handlebars helper function for checking whether a specified user ID is that of the current user
function isCurrentUser(userID, options) {
    if (isUserLoggedIn()) {
        if (userID == currentUser.UserID) {
            return options.fn(this); // true
        }
    }
        
    return options.inverse(this); // false
}

// Handlebars helper function for checking whether a comment is bookmarked by the current user
function isBookmarked(commentID, options) {
    if (bookmarkedCommentIDs && bookmarkedCommentIDs.includes(commentID)) {
        return options.fn(this); // true
    } else {
        return options.inverse(this); // false
    }
}

// Handlebars helper function for checking whether a comment has been liked by the current user
function isLiked(commentID, options) {
    if (userRatings) {
        for (var i = 0; i < userRatings.length; i++) {
            if (userRatings[i].CommentID == commentID) {
                return options.fn(this); // true
            }
        }
    }
    return options.inverse(this); // false
}

// Check if content of comment is valid before attempting to create it
// returns: true if valid, false otherwise
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
        synthModeEnabled = override;
    }
    else {
        synthModeEnabled = !synthModeEnabled;
    }
    showSynthButtons();
}

function showSynthButtons() {
    var commentOptions = $(".commentOptions");
    if (synthModeEnabled) {
        commentOptions.append("<a class='btnSynthesizeComment' href='javascript:;'><span class='fa fa-list-ul'></span> Synth this comment</a>")
    }
    else {
        $(".btnSynthesizeComment").remove();
    }
}

// Add a synthesis field to the reply box
function addSynth(commentId, placeholderText) {
    var duplicate = false;
    synthList.find("li").each(function () {
        if ($(this).data("commentid") == commentId) {
            duplicate = true;
            return;
        }
    });
    if (!placeholderText) {
        placeholderText = "";
    }
    if (!duplicate) {
        synthList.append(
            "<li data-commentid='" + commentId + "'>" +
                "<a href='javascript:;' class='synthDelete btn btn-small btn-purple'>X</a>" +
                "<span class='synthIdLabel'> #" + commentId + "</span>" +
                "<input name='synthSubject' type='text' placeholder='Enter a Short Description' value='"+placeholderText+"'/>" +
            "</li>"
        );
    }
}

// Create an array of SynthesisRequest objects from the synthesis fields in the reply box
// returns: an array of SynthesisRequest data transfer objects
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

// Close any existing reply or edit box
function closeAllCommentForms(){
    $(".hasReplyForm").each(function () {
        $(this).find(".btn-cancelReply").trigger("click");
    });
    $(".hasEditForm").each(function () {
        $(this).find(".btn-cancelEdit").trigger("click");
    });
}
