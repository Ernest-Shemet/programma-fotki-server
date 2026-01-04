#!/usr/bin/env python3
"""
Simple HTTP receiver for photo uploads.
Accepts POST /upload with multipart/form-data field "file" and saves it to output folder.
No external dependencies required.
"""

from http.server import HTTPServer, BaseHTTPRequestHandler
import cgi
import argparse
import os
import time
from pathlib import Path


class UploadHandler(BaseHTTPRequestHandler):
    def __init__(self, *args, output_dir: Path, **kwargs):
        self.output_dir = output_dir
        super().__init__(*args, **kwargs)

    def do_POST(self):
        if self.path != "/upload":
            self.send_error(404, "Not found")
            return

        content_type = self.headers.get("Content-Type", "")
        if "multipart/form-data" not in content_type:
            self.send_error(400, "Expected multipart/form-data")
            return

        form = cgi.FieldStorage(
            fp=self.rfile,
            headers=self.headers,
            environ={
                "REQUEST_METHOD": "POST",
                "CONTENT_TYPE": content_type,
            },
        )

        if "file" not in form:
            self.send_error(400, "Missing file field")
            return

        upload = form["file"]
        if not upload.file:
            self.send_error(400, "File payload missing")
            return

        filename = upload.filename or f"photo_{int(time.time())}.jpg"
        safe_name = os.path.basename(filename)
        destination = self.output_dir / safe_name
        with open(destination, "wb") as out:
            out.write(upload.file.read())

        self.send_response(200)
        self.send_header("Content-Type", "application/json")
        self.end_headers()
        self.wfile.write(f'{{"status":"ok","saved_as":"{destination.name}"}}'.encode())
        print(f"Saved: {destination}")

    def log_message(self, format, *args):
        # Keep console output concise
        print(f"[{self.address_string()}] {format % args}")


def serve(port: int, output_dir: Path):
    output_dir.mkdir(parents=True, exist_ok=True)

    def handler(*args, **kwargs):
        return UploadHandler(*args, output_dir=output_dir, **kwargs)

    httpd = HTTPServer(("", port), handler)
    print(f"Receiving on http://0.0.0.0:{port}/upload -> {output_dir}")
    httpd.serve_forever()


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Receive photo uploads via HTTP.")
    parser.add_argument("--port", type=int, default=5000, help="Port to listen on")
    parser.add_argument(
        "--output",
        type=Path,
        default=Path.cwd() / "uploads",
        help="Directory to save incoming files",
    )
    args = parser.parse_args()
    serve(args.port, args.output)
