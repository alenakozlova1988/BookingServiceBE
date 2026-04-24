function(ctx) {
  user_id: ctx.identity.id,
  email: ctx.identity.traits.email,
  event: "user_registered"
}