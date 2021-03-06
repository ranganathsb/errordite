(function() {

  jQuery(function() {
    var $body, Group, group;
    $body = $('section#groups');
    if ($body.length > 0) {
      group = null;
      $body.delegate('a.delete', 'click', function() {
        var $this;
        $this = $(this);
        this.group = new Group($this.closest('form'));
        this.group["delete"]();
        return false;
      });
      return Group = (function() {

        function Group($form) {
          this.$form = $form;
        }

        Group.prototype["delete"] = function() {
          var $frm;
          $frm = this.$form;
          return Errordite.Confirm.show("Are you sure you want to delete this group?", {
            okCallBack: function() {
              return $frm.submit();
            }
          });
        };

        return Group;

      })();
    }
  });

}).call(this);
