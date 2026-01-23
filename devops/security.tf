resource "aws_iam_group" "game_artists" {
  name = "GameArtists"
}

resource "aws_iam_group_policy" "game_artist_policy" {
  name = "game_artist_full_access"
  group = aws_iam_group.game_artists.name
  
  policy = jsonencode({
    "Version": "2012-10-17",  

    "Statement": [  
      {
        "Effect": "Allow",  

        "Action": [  
          "s3:PutObject",      
          "s3:GetObject",      
          "s3:DeleteObject",
          "s3:ListBucket",
          "s3:ListAllMyBuckets"
        ],

        "Resource": [
          "${aws_s3_bucket.vortex_s3_assets.arn}/*",
          "${aws_s3_bucket.vortex_s3_assets.arn}"
          // Point to the bucket definition in database.tf
        ]
      },
      {
        "Effect": "Allow",
        "Action": [
          "iam:ChangePassword",
          "iam:GetAccountPasswordPolicy"
        ],
        "Resource": "arn:aws:iam::*:user/$${aws:username}"
      },
      {
        "Effect": "Allow",
        "Action": [
          "s3:ListAllMyBuckets",
          "s3:GetBucketLocation"
        ],
        "Resource": "*"
      }
    ]
  })
}

// Add admin group and policy + backend dev

resource "aws_iam_user" "G_Clara" {
  name = "GARCIA.Clara"
}

resource "aws_iam_user_login_profile" "G_Clara_login" {
  user                    = aws_iam_user.G_Clara.name
  password_reset_required = true 
}

// add user account

resource "aws_iam_user_group_membership" "G_Clara_membership" {
  user = aws_iam_user.G_Clara.name
  groups = [aws_iam_group.game_artists.name]
}

// Access Key 

resource "aws_iam_access_key" "G_Clara_key" {
  user = aws_iam_user.G_Clara.name
}

// use to get aws url 
data "aws_caller_identity" "current" {}

output "G_Clara_credentials" {
  value = {
    username = aws_iam_user.G_Clara.name
    group = "GameArtists"

    console_url       = "https://${data.aws_caller_identity.current.account_id}.signin.aws.amazon.com/console"
    initial_password  = aws_iam_user_login_profile.G_Clara_login.password
    password_note     = "Warning you will have to change your password"

    access_key_id = aws_iam_access_key.G_Clara_key.id
    secret_access_key = aws_iam_access_key.G_Clara_key.secret
  }
  sensitive = true
}