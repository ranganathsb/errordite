
jQuery -> 
	$body = $('section#groups');

	if $body.length > 0
		group = null;

		$body.delegate('a.delete', 'click', () -> 
			$this = $ this
			this.group = new Group $this.closest('form')
			this.group.delete()
			false
		)	

		class Group
			constructor: ($form) -> 
				this.$form = $form

			delete: () -> 
				$frm = this.$form
				Errordite.Confirm.show("Are you sure you want to delete this group?", {
					okCallBack: -> 
						$frm.submit()
				})

	