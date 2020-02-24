function createBreadcrumbs(breadcrumbs) {
    $('#breadcrumb').toggleClass('d-none');
    for (var i = 0; i < breadcrumbs.length; i++) {
        $('.breadcrumb').append('<li class="breadcrumb-item ' + breadcrumbs[i].active + '">' + breadcrumbs[i].content + '</li>');
    }
}

function showSpinner(elementId) {
    $('#' + elementId).toggleClass('d-none');
}

function toggleModalDialog(dialogId) {
    $('#' + dialogId).modal('toggle');
}

// Groups Table
//function initializeGroupsTable() {
//    var table_name = '#groups';
//    var table = $(table_name).DataTable({
//        responsive: true,
//        "order": [[1, "asc"]],
//        "columnDefs": [
//            { "orderable": false, "targets": [0, 4] }
//        ]
//    });
//    var dataTable = $(table_name).dataTable();
//    // Search box
//    $('#searchbox').keyup(function () {
//        dataTable.fnFilter(this.value);
//    });
//    $('.dataTables_filter').addClass('d-none');
//    // Length
//    $('#entries_place').html($('.dataTables_length select').removeClass('custom-select-sm form-control-sm'));
//    $('.dataTables_length').addClass('d-none');
//    // Info
//    $('#showing_place').html($('.dataTables_info'));
//    // Paginate
//    $('#pagination_place').html($('.dataTables_paginate'));
//}

//function destroyLogsTable() {
//    $('#logs').DataTable().destroy();
//}

// Logs Table
function initializeLogsTable() {
    $('#logs').DataTable({
        "paging": false,
        "info": false,
        "order": [[0, "desc"]]
    });
    var dataTable = $('#logs').dataTable();
    $('#searchbox').keyup(function () {
        dataTable.fnFilter(this.value);
    });
    $('.dataTables_filter').addClass('d-none');

    $('.content-body').scrollTop(0);
}

function destroyLogsTable() {
    $('#logs').DataTable().destroy();
}