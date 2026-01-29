resource "aws_iam_group" "admin_group" {
  name = "VortexAdmin"
}

resource "aws_iam_group" "game_artists" {
  name = "GameArtists"
}

resource "aws_iam_group" "back_end_dev" {
  name = "BackEndDevs"
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

resource "aws_iam_group_policy" "admin_group_policy" {
  group  = aws_iam_group.admin_group.name
  policy = jsonencode(
    {
      "Version": "2012-10-17",
      "Statement": [
        {
          "Sid": "fulladministrative",
          "Effect": "Allow",
          "Action": "*",
          "Resource": "*"
        }
      ]
    }
  )
}

resource "aws_iam_group_policy" "back_end_policy" {
  name = "back-end-server-database-access"
  group = aws_iam_group.back_end_dev.name
  
  policy = jsonencode({
      Version = "2012-10-17"
      Statement = [
        {
          Sid      = "EC2ReadOnly"
          Effect   = "Allow"
          Action   = [
            "ec2:Describe*",    
            "ec2:Get*",          
            "ec2:List*"          
          ]
          Resource = "*"
        },
        {
          Sid      = "RDSReadOnly"
          Effect   = "Allow"
          Action   = [
            "rds:Describe*",    
            "rds:ListTagsForResource",
            "rds:DownloadDBLogFilePortion" 
          ]
          Resource = "*"
        },
        {
          Sid      = "GlobalConsoleAccess"
          Effect   = "Allow"
          Action   = [
            "tag:GetResources",
            "cloudwatch:GetMetricData",
            "cloudwatch:ListMetrics",
            "cloudwatch:GetMetricStatistics" 
          ]
          Resource = "*"
        },
        {
          "Effect": "Allow",
          "Action": [
            "iam:ChangePassword",
            "iam:GetAccountPasswordPolicy"
          ],
          "Resource": "arn:aws:iam::*:user/$${aws:username}"
        }
      ]
    })
}

// Add admin group and policy + backend dev

resource "aws_iam_user" "G_Clara" {
  name = "GARCIA.Clara"
}

resource "aws_iam_user" "J_Mikael" {
  name = "JARREAU.Mikael"
}

resource "aws_iam_user" "M_Alex" {
  name = "MIVELAZ.Alex"
}

resource "aws_iam_user" "L_Maxime" {
  name = "LOMBARD.Maxime"
}

resource "aws_iam_user_login_profile" "G_Clara_login" {
  user                    = aws_iam_user.G_Clara.name
  password_reset_required = true 
}

resource "aws_iam_user_login_profile" "J_Mikael_login" {
  user = aws_iam_user.J_Mikael.name
  password_reset_required = true
}

resource "aws_iam_user_login_profile" "M_Alex_login" {
  user = aws_iam_user.M_Alex.name
  password_reset_required = true
}

resource "aws_iam_user_login_profile" "L_Maxime_login" {
  user = aws_iam_user.L_Maxime.name
  password_reset_required = true
}

// add user account

resource "aws_iam_user_group_membership" "G_Clara_membership" {
  user = aws_iam_user.G_Clara.name
  groups = [aws_iam_group.game_artists.name]
}

resource "aws_iam_user_group_membership" "J_Mikael_membership" {
  user = aws_iam_user.J_Mikael.name
  groups = [aws_iam_group.back_end_dev.name]
}

resource "aws_iam_user_group_membership" "M_Alex_membership" {
  user = aws_iam_user.M_Alex.name
  groups = [aws_iam_group.admin_group.name]
}

resource "aws_iam_user_group_membership" "L_Maxime_membership" {
  user = aws_iam_user.L_Maxime.name
  groups = [aws_iam_group.admin_group.name]
}

// Access Key 

resource "aws_iam_access_key" "G_Clara_key" {
  user = aws_iam_user.G_Clara.name
}

resource "aws_iam_access_key" "J_Mikael_key" {
  user = aws_iam_user.J_Mikael.name
}

resource "aws_iam_access_key" "M_Alex_key" {
  user = aws_iam_user.M_Alex.name
}

resource "aws_iam_access_key" "L_Maxime_key" {
  user = aws_iam_user.L_Maxime.name
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

output "J_Mikael_credentials" {
  value = {
    username = aws_iam_user.J_Mikael.name
    group = "BackEndDevs"

    console_url       = "https://${data.aws_caller_identity.current.account_id}.signin.aws.amazon.com/console"
    initial_password  = aws_iam_user_login_profile.J_Mikael_login.password
    password_note     = "Warning you will have to change your password"

    access_key_id = aws_iam_access_key.J_Mikael_key.id
    secret_access_key = aws_iam_access_key.J_Mikael_key.secret
  }
  sensitive = true
}

output "M_Alex_credentials" {
  value = {
    username = aws_iam_user.M_Alex.name
    group = "Admin"

    console_url       = "https://${data.aws_caller_identity.current.account_id}.signin.aws.amazon.com/console"
    initial_password  = aws_iam_user_login_profile.M_Alex_login.password
    password_note     = "Warning you will have to change your password"

    access_key_id = aws_iam_access_key.M_Alex_key.id
    secret_access_key = aws_iam_access_key.M_Alex_key.secret
  }
  sensitive = true
}

output "L_Maxime_credentials" {
  value = {
    username = aws_iam_user.L_Maxime.name
    group = "Admin"

    console_url       = "https://${data.aws_caller_identity.current.account_id}.signin.aws.amazon.com/console"
    initial_password  = aws_iam_user_login_profile.L_Maxime_login.password
    password_note     = "Warning you will have to change your password"

    access_key_id = aws_iam_access_key.L_Maxime_key.id
    secret_access_key = aws_iam_access_key.L_Maxime_key.secret
  }
  sensitive = true
}
