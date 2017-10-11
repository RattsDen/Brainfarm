var newComments = null;
var newCommentsDiff = null;

$(document).ready(function () {
    // Button click event
    $(document).on("click", "#btn-fetch-comments", function () {
        if (newCommentsDiff == null) {
            checkForNewComments(true);
        } else {
            addCommentsToView(newCommentsDiff);
            allComments = newComments;
        }
        // Reset new comment holding variables
        newComments = null;
        newCommentsDiff = null;
        // Reset button text
        $("#btn-fetch-comments").text("Fetch New Comments");
    });

    // Check for new comments on an interval
    window.setInterval(function () {
        console.log("check");
        var addAutomatically = $("#chk-fetch-auto").is(":checked");
        checkForNewComments(addAutomatically);
    }, 5000); // Every 5 seconds //TODO: too fast - for demo purposes only
});

// Query the Brainfarm service for the current comment list and pick out the new comments
// input addAutomatically: boolean - whether or not new comments should automatically be
//added to the DOM
function checkForNewComments(addAutomatically) {
    var args = {
        projectID: projectID, // projectID is global variable in Project.aspx
        parentCommentID: null
    };
    serviceAjax("GetComments", args, function(results) {
        // newCommentsDiff is a global variable
        newCommentsDiff = compareCommentTrees(allComments, results);
        if (newCommentsDiff.length != 0) {
            // newComments is a global vaiable
            newComments = results;
            if (addAutomatically) {
                addCommentsToView(newCommentsDiff);
                allComments = newComments;
            } else {
                // Set button text
                var newCommentCount = countComments(newComments) - countComments(allComments);
                $("#btn-fetch-comments").text(newCommentCount
                    + " New Comment"
                    + (newCommentCount > 1 ? "s" : ""));
            }
        }
    }, function(error) {
        console.log(error);
    });
}

// Count the total number of comments in an array of comments, including children
function countComments (comments) {
    var count = 0;
    if (comments != null) {
        count += comments.length;
        for (var i = 0; i < comments.length; i++) {
            count += countComments(comments[i].Children);
        }
    }
    return count;
}

// Compare two comment trees to find the comments that only exist in the second tree.
//The children of new comments will not be contained in the returned array, however 
//these child comments can still be accessed via the "Children" array of each parent.
//Effectively this function returns an array of new comment trees.
// input base: the old tree
// input compare: the new tree for which to find new comments in
// returns: an array of comments
function compareCommentTrees(base, compare) {
    var diff = [];
    for (var i = 0; i < compare.length; i++) {
        var thisCommentIsNew = false;
        if (!commentTreeContainsId(base, compare[i].CommentID)) {
            diff.push(compare[i]);
            thisCommentIsNew = true;
        }
        // If this comment is new then it's children are also new and will be 
        //added automatically when this comment is added
        if (!thisCommentIsNew) {
            diff.push.apply(diff, compareCommentTrees(base, compare[i].Children));
        }
    }
    return diff;
}

// Check whether or not a given comment tree contains a comment the specified ID
// returns: true if ID is contained in tree, false otherwise
function commentTreeContainsId(comments, commentId) {
    if (comments != null) {
        for (var i = 0; i < comments.length; i++) {
            if (comments[i].CommentID == commentId) {
                return true;
            } else if (commentTreeContainsId(comments[i].Children, commentId)) {
                return true;
            }
        }
    }
    return false;
}

// Add comments to the view (DOM) using the comment template defined in Project.js
function addCommentsToView(comments) {
    for (var i = 0; i < comments.length; i++) {
        var parentCommentID = comments[i].ParentCommentID;
        var target = $("#" + parentCommentID + ">.commentChildren");
        if (target != null) {
            var html = commentTemplate([comments[i]]); // Comment template is defined in Project.js
            target.append(html);
            $("#" + comments[i].CommentID).hide();
            $("#" + comments[i].CommentID).show(1000);
        }
    }
}
