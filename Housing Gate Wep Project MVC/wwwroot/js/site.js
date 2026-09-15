// App shell: mobile sidebar toggle.
$(function () {
    var $sidebar = $('#appSidebar');
    var $backdrop = $('#sidebarBackdrop');

    if ($sidebar.length) {
        $('.sidebar-toggle').on('click', function () {
            $sidebar.toggleClass('open');
            $backdrop.toggleClass('show');
            $('body').toggleClass('overflow-hidden');
        });

        $backdrop.on('click', function () {
            $sidebar.removeClass('open');
            $backdrop.removeClass('show');
            $('body').removeClass('overflow-hidden');
        });

        $(window).on('resize', function () {
            if ($(window).width() >= 992) {
                $sidebar.removeClass('open');
                $backdrop.removeClass('show');
                $('body').removeClass('overflow-hidden');
            }
        });
    }

    // Upload zone drag highlight.
    $(document).on('dragover dragenter', '.upload-zone', function (e) {
        e.preventDefault();
        $(this).addClass('dragover');
    });
    $(document).on('dragleave dragend drop', '.upload-zone', function (e) {
        e.preventDefault();
        $(this).removeClass('dragover');
    });
    $(document).on('drop', '.upload-zone', function (e) {
        e.preventDefault();
        var input = $(this).find('input[type=file]')[0];
        if (input && e.originalEvent.dataTransfer) {
            input.files = e.originalEvent.dataTransfer.files;
            $(input).trigger('change');
        }
    });

    // Property gallery thumbnails.
    $(document).on('click', '.property-gallery-thumb', function () {
        var src = $(this).data('full');
        if (src) {
            $('#galleryMain').attr('src', src);
        }
        $(this).closest('.gallery-thumbs').find('.property-gallery-thumb').removeClass('active');
        $(this).addClass('active');
    });
});

// Room editor on the owner property form.
$(function () {
    function reindexRooms() {
        $('#roomsContainer .room-row').each(function (index) {
            $(this).find('[name]').each(function () {
                var name = $(this).attr('name');
                if (name) {
                    name = name.replace(/Rooms\[\d+\]/g, 'Rooms[' + index + ']');
                    $(this).attr('name', name);
                }
            });
        });
    }

    $('#addRoom').on('click', function () {
        var index = $('#roomsContainer .room-row').length;
        var html = $('#roomTemplate').html().replace(/__INDEX__/g, index);
        $('#roomsContainer').append(html);
    });

    $(document).on('click', '.remove-room', function () {
        $(this).closest('.room-row').remove();
        reindexRooms();
    });
});
