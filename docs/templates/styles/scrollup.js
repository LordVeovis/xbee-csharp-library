// https://cybmeta.com/scroll-arriba-jquery
(function($) {
  $(document).ready(
    function() {
      // Show or hide the button if we are 200 px
      // below the top position or if we are scrolling up.
      var lastScroll = 0;
      $(window).scroll(function() {
        var currentScroll = $(window).scrollTop();
        if (currentScroll < 200 || currentScroll > lastScroll) {
          $('.scrollup').fadeOut();
        } else {
          $('.scrollup').fadeIn();
        }
        lastScroll = currentScroll;
      });

      // Animate the scroll when clicking.
      $('.scrollup').click( function(e) {
        e.preventDefault();
        $('html, body').animate({scrollTop : 0}, 300);
      });
    });
})(jQuery);
