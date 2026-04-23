import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { LojaContextService } from "./services/loja-context.service";

@Component({
  template: '<loja-nav></loja-nav><router-outlet></router-outlet>'
})
export class LojaShellComponent implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private ctx: LojaContextService
  ) {}

  ngOnInit() {
    const slug = this.route.snapshot.parent?.paramMap.get('slug');
    if (slug) this.ctx.initFromSlug(slug).subscribe();
    else this.ctx.initFromSession();
  }
}