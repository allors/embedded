import { expect, test } from "vitest";
import { Meta } from "../../src/meta/Meta";

test("Meta", () => {
  let meta = new Meta();
  expect(meta).toBeDefined();
});
