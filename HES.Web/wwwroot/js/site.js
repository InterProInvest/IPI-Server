$(document).ready(function () {
    $('.loading').removeClass('d-flex');
    $('.loading').addClass('d-none');

    var toggled = localStorage.getItem('ToggledSidebar');
    if (toggled === 'true') {
        ToggleSidebarClass();
    }

    $('#collapseAudit').on('show.bs.collapse', function () {
        $('#collapseSettings').collapse('hide');
    });

    $('#collapseSettings').on('show.bs.collapse', function () {
        $('#collapseAudit').collapse('hide');
    });
});

function ToggleSidebar() {
    ToggleSidebarClass();

    var toggled = localStorage.getItem('ToggledSidebar');
    if (toggled === 'true') {
        localStorage.setItem('ToggledSidebar', 'false');
    }
    else {
        localStorage.setItem('ToggledSidebar', 'true');
    }
}

function ToggleSidebarClass() {
    $('.custom-sidebar').toggleClass('toggled');
    $('.page_labels').toggleClass('toggled');
    $('.sidebar-settings').toggleClass('toggled');
    $('.icon-arrow').toggleClass('toggled');
    $('.copyright').toggleClass('toggled');
    $('.loading').toggleClass('toggled');
    $('.icon-hideez').toggleClass('toggled');
}

// ~ Common ~
// Validate Partial View
function ValidatePartialView() {
    $('form').removeData('validator');
    $('form').removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse('form');
}

// Clear content on modal dialog hide
$('#modalDialog').on('hidden.bs.modal', function (e) {
    $("#modalBody").html("<div class='d-flex justify-content-center align-items-center'><div class='spinner-grow text-primary' style='width: 3rem; height: 3rem;' role='status'><span class='sr-only'></span></div></div>");
});

// Device Access Profile
// Initialize DataTables
function InitDeviceAccessProfileDT() {
    var table_name = '#deviceAccessProfiles';
    var table = $(table_name).DataTable({
        responsive: true,
        "order": [[1, "asc"]],
        "columnDefs": [
            { "orderable": false, "targets": [0, 5] }
        ]
    });
    var dataTable = $(table_name).dataTable();
    // Search box
    $('#searchbox').keyup(function () {
        dataTable.fnFilter(this.value);
    });
    $('.dataTables_filter').addClass('d-none');
    // Length
    $('#entries_place').html($('.dataTables_length select').removeClass('custom-select-sm form-control-sm'));
    $('.dataTables_length').addClass('d-none');
    // Info
    $('#showing_place').html($('.dataTables_info'));
    // Paginate
    $('#pagination_place').html($('.dataTables_paginate'));
}

// Breadcrumbs
function DeviceAccessProfileSetBreadcrumbs() {
    $('#breadcrumb').toggleClass('d-none');
    $('.breadcrumb').append('<li class="breadcrumb-item active">Settings</li>');
    $('.breadcrumb').append('<li class="breadcrumb-item active">Device Access Profiles</li>');
}

// Pin connection checkbox
$(document).on('change', '#PinConnection', function () {
    if (this.checked) {
        $('#PinBonding').prop('checked', true);
    }
});

// Pin new channel checkbox
$(document).on('change', '#PinNewChannel', function () {
    if (this.checked) {
        $('#PinBonding').prop('checked', true);
    }
});

// Dismiss uncheck Pin bonding
$(document).on('change', '#PinBonding', function () {
    if ($('#PinConnection').prop("checked") || $('#PinNewChannel').prop("checked")) {
        $('#PinBonding').prop('checked', true);
    }
});

// Slider Pin expiration
$(document).on('input', '#pin_expiration', function () {
    if (this.value <= 59) {
        $('#pin_expiration_value').html(this.value + ' min');
    }
    else {
        $('#pin_expiration_value').html((this.value - 59) + ' hrs');
    }
});

// Slider Pin length
$(document).on('input', '#pin_length', function () {
    $('#pin_length_value').html(this.value);
});

// Slider Pin try count
$(document).on('input', '#pin_try_count', function () {
    $('#pin_try_count_value').html(this.value);
});

// Init sliders text
function InitSlidersText() {
    // pin_expiration
    var pinExpiration = $('#pin_expiration').val();
    if (pinExpiration <= 59) {
        $('#pin_expiration_value').html(pinExpiration + ' min');
    }
    else {
        $('#pin_expiration_value').html((pinExpiration - 59) + ' hrs');
    }
    // pin_length
    $('#pin_length_value').html($('#pin_length').val());
    // pin_try_count
    $('#pin_try_count_value').html($('#pin_try_count').val());
}

// Device Licenses
// Initialize DataTables
function InitDeviceLicensesDT() {
    var table_name = '#deviceLicenses';
    var table = $(table_name).DataTable({
        responsive: true,
        "order": [[0, "asc"]]
    });
    var dataTable = $(table_name).dataTable();
    // Search box
    $('#searchbox').keyup(function () {
        dataTable.fnFilter(this.value);
    });
    $('.dataTables_filter').addClass('d-none');
    // Length
    $('#entries_place').html($('.dataTables_length select').removeClass('custom-select-sm form-control-sm'));
    $('.dataTables_length').addClass('d-none');
    // Info
    $('#showing_place').html($('.dataTables_info'));
    // Paginate
    $('#pagination_place').html($('.dataTables_paginate'));
}

// Breadcrumbs
function DeviceLicensesSetBreadcrumbs() {
    $('#breadcrumb').toggleClass('d-none');
    $('.breadcrumb').append('<li class="breadcrumb-item active">Settings</li>');
    $('.breadcrumb').append('<li class="breadcrumb-item active">Device Licenses</li>');
}

// DataTables initialization
function InitDeviceTasksDT() {
    var table_name = '#deviceTasks';
    var table = $(table_name).DataTable({
        responsive: true,
        "order": [[2, "desc"]]
    });
    var dataTable = $(table_name).dataTable();
    // Search box
    $('#searchbox').keyup(function () {
        dataTable.fnFilter(this.value);
    });
    $('.dataTables_filter').addClass('d-none');
    // Length
    $('#entries_place').html($('.dataTables_length select').removeClass('custom-select-sm form-control-sm'));
    $('.dataTables_length').addClass('d-none');
    // Info
    $('#showing_place').html($('.dataTables_info'));
    // Paginate
    $('#pagination_place').html($('.dataTables_paginate'));
}