var filterIsApplied = false;

$(document).ready(function () {

    // Apply filter clicked
    $(document).on("click", "#btn-filter-apply", function () {
        applyFilter();
    });

    // Remove filter clicked
    $(document).on("click", "#btn-filter-remove", function () {
        removeFilter();
    });

});

function applyFilter() {
    filterIsApplied = true;
    // All comments is technically an array, even though 
    //it should only have one element being the root of the tree
    for (var i = 0; i < allComments.length; i++) {
        filterCommentTree(allComments[i]);
    }
}

function removeFilter() {
    filterIsApplied = false;
    // All comments is technically an array, even though 
    //it should only have one element being the root of the tree
    for (var i = 0; i < allComments.length; i++) {
        removeFiltersFromCommentTree(allComments[i]);
    }
}


// Applies filter styling to a tree of comments
// input: root of the tree
// returns: true if this comment or any of its children are visible
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
        || (showBookmarked && bookmarkedCommentIDs.includes(comment.CommentID)
           // None selected: show all
        || !(showNormal || showSynth || showSpec || showContrib || showBookmarked));
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