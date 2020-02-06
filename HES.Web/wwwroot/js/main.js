function createBreadcrumbs(breadcrumbs) {
    $('#breadcrumb').toggleClass('d-none');
    for (var i = 0; i < breadcrumbs.length; i++) {
        $('.breadcrumb').append('<li class="breadcrumb-item ' + breadcrumbs[i].active + '">' + breadcrumbs[i].content + '</li>');
    }
}

function showSpinner(elementId) {
    $('#' + elementId).toggleClass('d-none');
}