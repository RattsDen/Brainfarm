$(document).ready(function () {

    // Sort radio button toggled
    $(document).on("change", ".rad-sort", function () {
        switch (this.id) {
            case "rad-sort-date-asc":
                sortComments(allComments, sortByDateAsc);
                break;
            case "rad-sort-date-desc":
                sortComments(allComments, sortByDateDesc);
                break;
            case "rad-sort-score":
                sortComments(allComments, sortByScore);
                break;
        }
        // processComments is defined in Project.js
        processComments(allComments);
        // If a filter is active, re-apply it after the sort
        // filterIsApplied and applyFilter() are defined in ProjectFiltering.js
        if (filterIsApplied) {
            applyFilter();
        }
    });

});


// Sort sibling comments according to the provided comparator function
// This function calls itself recursively for each comment's children
function sortComments(comments, comparator) {
    // Recursivly sort children
    for (var i = 0; i < comments.length; i++) {
        sortComments(comments[i].Children, comparator);
    }
    // Sort comments in passed array
    comments.sort(comparator);
}

// Comparator function for sorting by comment creation date, descending
function sortByDateDesc(a, b) {
    var aMillis = parseInt(a.CreationDate.substring(6));
    var bMillis = parseInt(b.CreationDate.substring(6));
    return bMillis - aMillis;
}

// Comparator function for sorting by comment creation date, ascending
function sortByDateAsc(a, b) {
    var aMillis = parseInt(a.CreationDate.substring(6));
    var bMillis = parseInt(b.CreationDate.substring(6));
    return aMillis - bMillis;
}

// Comparator function for sorting by comment score
function sortByScore(a, b) {
    return b.Score - a.Score;
}